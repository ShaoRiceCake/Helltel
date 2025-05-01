// Controller层核心逻辑
using System;
using UnityEngine;

/// <summary>
/// 游戏逻辑控制器（挂载到场景持久化物体）
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("数据引用")]
    public GameDataModel _gameData;

    public static GameController Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        _gameData.ResetData();

    }
    private void OnEnable() {
        
    }
    // 增加血上限接口
    public void AddMaxHealth(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("请使用 DeductHealth 方法扣除血量");
            return;
        }
        
        _gameData.Money += amount;
    }
    // 扣血血上限接口（这个一般没用）
    public void DeductMaxHealth(int amount)
    {
        if (_gameData.MaxHealth >= amount)
        {
            _gameData.MaxHealth -= amount;
        }
        else
        {
            Debug.LogError("血上限不能是负数");
            return;
        }
   
    }

    // 增加血量接口
    public void AddHealth(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("请使用 DeductHealth 方法扣除血量");
            return;
        }
        _gameData.Money += amount;
    }

    // 扣血接口
    public void DeductHealth(int amount)
    {
        if (_gameData.Health >= amount)
        {
            _gameData.Health -= amount;
        }
        else
        {
            Debug.Log("死亡");
            return;
        }
      
    }

    // 增加钱接口
    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("请使用 DeductMoney 方法扣除金钱");
            return;
        }
        
        _gameData.Money += amount;
    }

    // 扣钱接口
    public void DeductMoney(int amount)
    {
        if (_gameData.Money >= amount)
        {
            _gameData.Money -= amount;
        }
        //暂定钱可以是负数
        _gameData.Money -= amount;
    }
    //增加绩效接口
    public void AddPerformance(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("请使用 DeductMoney 方法扣除绩效");
            return;
        }
        _gameData.Performance += amount;
    }
    // 扣绩效接口
    public void DeductPerformance(int amount)
    {
        if (_gameData.Performance >= amount)
        {
            _gameData.Performance -= amount;
        }
        //暂定绩效可以是负数
        _gameData.Performance -= amount;
    }


}