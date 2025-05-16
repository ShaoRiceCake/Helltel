// MonsterSpawnSystem.cs

using System;
using System.Collections;
using System.Collections.Generic;
using Helltal.Gelercat;
using NUnit.Framework;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class MonsterSpawnSystem : MonoBehaviour
{
    //============== 核心参数配置区 ==============

    [Header("游戏基础熵值")]
    public float baseEntropy = 20f;
    [Header("每层变化的熵值")]
    public float floorMultiplier = 8f;


    [Header("基础客梯数")]
    public int baseGuestElevatorCount = 8;
    [Header("每层增加的客梯数")]
    public int guestElevatorFloorMultiplier = 3;

    [Header("生成设置")]
    public float checkInterval = 30f;
    //最大尝试次数
    private int maxAttempts = 5;
    [Header("可生成的怪物池")]
    public List<GameObject> monsterPrefabs;
    private GameDataModel _data;

    //============== 运行时状态 ==============
    private List<Transform> activeGuestElevators = new(); // 可用客梯列表
    private float floorTimeCounter;                      // 本层停留计时（本关持续的时间）
    private float totalMoney;                            // 累计金钱
    //private int totalDeaths;                             // 死亡次数
    private Dictionary<GameObject, int> spawnRecords = new(); // 生成记录
    private List<GuestBase> activeMonsters = new();        // 存活怪物列表

    //============== 初始化流程 ==============
    void Start()
    {
        _data = Resources.Load<GameDataModel>("GameData");
        
        InitializeSpawnData();
        StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// 客梯我还没做呢……之后再写
    /// 初始化客梯梯系统
    /// 1. 收集场景中的客梯
    /// 2. 根据楼层规则筛选
    /// </summary>
    void InitializeGuestElevators()
    {
        // 收集所有标记的客梯
        var allGuestElevators = GameObject.FindGameObjectsWithTag("GuestElevator");
        foreach (var ge in allGuestElevators)
        {
            activeGuestElevators.Add(ge.transform);
        }

        // 计算允许的客梯数量
        int maxGuestElevators = Mathf.Max(
            baseGuestElevatorCount + (_data.Level * guestElevatorFloorMultiplier), 
            1 // 保证至少1个客梯
        );

        // 随机移除多余的客梯
        while (activeGuestElevators.Count > maxGuestElevators)
        {
            int index = Random.Range(0, activeGuestElevators.Count);
            Destroy(activeGuestElevators[index].gameObject);
            activeGuestElevators.RemoveAt(index);
        }
    }

    /// <summary>
    /// 初始化生成记录数据
    /// </summary>
    void InitializeSpawnData()
    {
        spawnRecords.Clear();
        foreach (var prefab in monsterPrefabs)
        {
            spawnRecords[prefab] = 0;
        }
    }

    //============== 核心生成逻辑 ==============
    /// <summary>
    /// 周期性生成检测协程
    /// </summary>
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            AttemptSpawnProcess();
        }
    }

    /// <summary>
    /// 单次生成尝试流程
    /// </summary>
    void AttemptSpawnProcess()
    {
        float currentEntropy = CalculateCurrentEntropy();
        float entropyLimit = CalculateEntropyLimit();
        Debug.Log(currentEntropy +"与"+entropyLimit);
        //获取随机排序的怪物生成列表（
        var candidates = GetShuffledCandidates();

        for (int i = 0; i < Mathf.Min(maxAttempts, candidates.Count); i++)
        {
            GameObject prefab = candidates[i];
            GuestBase monster = prefab.GetComponent<GuestBase>();

            if (CanSpawn(monster, currentEntropy, entropyLimit))
            {
                ExecuteSpawn(prefab, monster);
                return;
            }
        }
    }

    /// <summary>
    /// 生成条件检查
    /// </summary>
    bool CanSpawn(GuestBase monster, float currentEntropy, float limit)
    {
        //注释这段是因为怪物基类里还没有这几个变量
        // 检查生成次数限制
        bool underSpawnLimit = monster.maxSpawnCount <= 0 || 
                             spawnRecords[monster.gameObject] < monster.maxSpawnCount;

        // 检查熵值限制
        bool underEntropyLimit = (currentEntropy + monster.entropyValue) <= limit;

        return underSpawnLimit && underEntropyLimit;
    }
    
    /// <summary>
    /// 执行生成操作
    /// </summary>
    private void ExecuteSpawn(GameObject prefab, GuestBase monster)
    {
        NavPointsManager navPointsManager = FindObjectOfType<NavPointsManager>();

        List<NavPoint> navPoints = navPointsManager.GetNavPoints();
        int randomIndex = Random.Range(0,navPoints.Count);

        Transform randomNavPoint = navPoints[randomIndex].transform;
        Debug.Log("生成"+prefab+"在第"+randomIndex+"个navPoint处");
        Vector3 spawnPoint = randomNavPoint.position; 
        Debug.Log("生成在"+spawnPoint);

        // 实例化并记录
        var instance = Instantiate(prefab, spawnPoint, Quaternion.identity,this.gameObject.transform);
        if (NetworkManager.Singleton)
        {
            instance.GetComponent<NetworkObject>().Spawn();
        }

        activeMonsters.Add(instance.GetComponent<GuestBase>());
        spawnRecords[prefab]++;
    }

    //============== 工具方法 ==============
    /// <summary>
    /// 获取随机排序的候选列表
    /// </summary>
    List<GameObject> GetShuffledCandidates()
    {
        List<GameObject> list = new List<GameObject>(monsterPrefabs);
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
        return list;
    }

    /// <summary>
    /// 计算当前熵值上限
    /// </summary>
    float CalculateEntropyLimit()
    {
        return baseEntropy 
            + _data.Level * floorMultiplier;
 
    }

    /// <summary>
    /// 计算当前总熵值
    /// </summary>
    float CalculateCurrentEntropy()
    {
        float total = 0f;
        foreach (var m in activeMonsters)
        {
            //这里是挨个相加每个怪物的熵值得出当前总熵值
            total += m.entropyValue;
        }
        return total;
    }

    //============== 事件接口 ==============
    /// <summary>
    /// 怪物被击败时调用
    /// </summary>
    public void RegisterMonsterDefeat(GuestBase monster)
    {
        activeMonsters.Remove(monster);
        
    }
    private void CleanupBeforeNextLevel()
    {
        // 清理所有存活怪物
        foreach (var monster in activeMonsters.ToArray()) 
        {
            if (monster) Destroy(monster.gameObject);
        }
        activeMonsters.Clear();

        // 强制GC回收（可选）
        System.GC.Collect();
    }

    private void OnDestroy()
    {
        CleanupBeforeNextLevel();
    }
}