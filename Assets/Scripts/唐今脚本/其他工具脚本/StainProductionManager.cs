using UnityEngine;
using System.Collections.Generic;
using Obi;
using UnityEngine.Serialization;

public class StainProductionManager : MonoBehaviour
{
    [Header("Emitter Settings")]
    public ObiEmitterManager emitterManager;
    public bool specialEmissionMode; // 特殊发射开关

    [Header("Emission Parameters")]
    public Vector2Int countRange = new Vector2Int(5, 10);
    public Vector2 speedRange = new Vector2(1f, 3f);
    public Vector2 randomnessRange = new Vector2(0.1f, 0.5f);
    public float emissionInterval = 0.2f;
    public bool randomRotation = true;

    public List<Transform> emissionPositions = new List<Transform>();
    private int _currentIndex;
    private float _timer;
    private int _totalEmittedCount;
    private int _requiredEmissionTarget;
    private bool _isEmitting;
    private bool _isDungeonReady;
    private bool _isInitialized;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_isInitialized) return;
        
        _isInitialized = true;
        GameController.Instance._gameData.OnFloorChanged += OnFloorChanged;
        DungeonGenerator.OnDungeonBuildCompleted += OnDungeonBuilt;
    }

    private void OnDestroy()
    {
        if (!_isInitialized) return;
        
        // 清理事件订阅
        GameController.Instance._gameData.OnFloorChanged -= OnFloorChanged;
        DungeonGenerator.OnDungeonBuildCompleted -= OnDungeonBuilt;
    }
    private void OnFloorChanged()
    {
        ResetAllState();
    }

    private void OnDungeonBuilt(DungeonGenerator generator)
    {
        // 地牢生成完成后才准备污渍布置
        if (GameController.Instance._gameData.CurrentLoadedScene != GameController.Instance._gameData.dungeon) return;
        _isDungeonReady = true;
        PrepareStainProduction();
    }
    private void PrepareStainProduction()
    {
        // 查找所有标记为StainProduction的对象
        var stainObjects = GameObject.FindGameObjectsWithTag("StainProduction");
        emissionPositions.Clear();
        
        foreach (var obj in stainObjects)
        {
            emissionPositions.Add(obj.transform);
        }

        // 如果没有找到任何标记对象，添加默认位置
        if (emissionPositions.Count == 0)
        {
            Debug.LogWarning("No objects with 'StainProduction' tag found. Using default position.");
            emissionPositions.Add(transform);
        }

        // 设置发射目标
        _requiredEmissionTarget = GameController.Instance._gameData.PerformanceTarget * 
                                 (specialEmissionMode ? 4 : 2);
        _totalEmittedCount = 0;
        _isEmitting = true;
    }

    private void Update()
    {
        if (!_isEmitting || emissionPositions.Count == 0 || !emitterManager) 
            return;

        // 检查是否达到目标
        if (_totalEmittedCount >= _requiredEmissionTarget)
        {
            _isEmitting = false;
            Debug.Log("Stain production completed. Total emitted: " + _totalEmittedCount);
            return;
        }

        // 定时发射
        _timer += Time.deltaTime;
        if (!(_timer >= emissionInterval)) return;
        _timer = 0f;
        EmitAtRandomPosition();
    }

    private void EmitAtRandomPosition()
    {
        if (emissionPositions.Count == 0) return;

        // 随机选择发射位置
        var randomIndex = Random.Range(0, emissionPositions.Count);
        var emissionPoint = emissionPositions[randomIndex];
        
        // 随机参数
        var count = Random.Range(countRange.x, countRange.y + 1);
        var speed = Random.Range(speedRange.x, speedRange.y);
        var randomness = Random.Range(randomnessRange.x, randomnessRange.y);
        var rotation = randomRotation ? Random.rotation : emissionPoint.rotation;

        // 发射粒子
        emitterManager.EmitAtPosition(
            emissionPoint.position, 
            rotation, 
            count, 
            speed, 
            randomness
        );

        // 更新计数
        _totalEmittedCount += count;
    }

    private void ResetAllState()
    {
        // 安全地清理粒子
        if (emitterManager != null && emitterManager.gameObject.activeInHierarchy)
        {
            emitterManager.KillAllParticles();
        }
        
        // 重置所有状态变量
        _timer = 0f;
        _totalEmittedCount = 0;
        _isEmitting = false;
        _isDungeonReady = false;
        emissionPositions.Clear();
    }

    // 公开方法用于手动控制特殊发射模式
    public void SetSpecialEmissionMode(bool active)
    {
        specialEmissionMode = active;
    }

    // 公开方法用于手动开始污渍布置
    public void StartStainProduction()
    {
        if (GameController.Instance._gameData.CurrentLoadedScene == 
            GameController.Instance._gameData.dungeon)
        {
            PrepareStainProduction();
        }
    }
}