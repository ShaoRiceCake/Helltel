//Model层核心数据
using UnityEngine;
using System;

// 全局游戏数据模型
[CreateAssetMenu(fileName = "GameData", menuName = "Helltel/Data/GameData")]
public class GameDataModel : ScriptableObject
{
    [Header("绩效参数")]
    [SerializeField, Range(1.1f, 2f)] private float _performanceGrowth = 1.2f;
    [SerializeField] private int _baseTarget = 100;

    // 私有字段配合属性保护数据
    private int _money;
    private int _performance;
    private int _day = 1;
    private int _level = 0;
    private Dictionary<string, PlayerRuntimeData> _players = new();

    // 公开事件
    public event Action<int> OnMoneyChanged;      // 金钱变化
    public event Action<int> OnDayChanged;        // 天数变化
    public event Action<int> OnPerformancePassed;  // 绩效达标 
    public event Action OnPerformanceFailed;      // 绩效失败
    public event Action<int> OnLevelChanged;      // 层级变化

    // 玩家运行时数据类
    public class PlayerRuntimeData
    {
        public int Health { get; private set; } = 100;
        public event Action<int> OnHealthChanged;

        public void ModifyHealth(int delta)
        {
            Health = Mathf.Clamp(Health + delta, 0, 100);
            OnHealthChanged?.Invoke(Health);
        }
    }

    // 属性封装（数据访问入口）
    public int Money {
        get => _money;
        set {
            _money = Mathf.Max(0, value);
            OnMoneyChanged?.Invoke(_money); // 触发UI更新
        }
    }

    public int Performance {
        get => _performance;
        set {
            _performance = Mathf.Max(0, value);
            CheckPerformance(); // 数值变化时自动检查绩效
        }
    }

    public int PerformanceTarget { get; set; }

    public int CurrentDay {
        get => _day;
        set {
            _day = value;
            OnDayChanged?.Invoke(_day);
        }
    }

    // 初始化方法
    public void ResetData()
    {
        Money = 0;
        Performance = 0;
        PerformanceTarget = _baseTarget;
        CurrentDay = 1;
        _level = 0;
    }

    // 内部绩效检查
    private void CheckPerformance()
    {
        if (Performance >= PerformanceTarget)
        {
            PerformanceTarget = Mathf.FloorToInt(PerformanceTarget * _performanceGrowth);
            OnPerformancePassed?.Invoke(PerformanceTarget);
        }
        else 
        {
            OnPerformanceFailed?.Invoke();
        }
    }
    //玩家管理接口
    public void RegisterPlayer(string playerId)
    {
        if (!_players.ContainsKey(playerId))
        {
            _players[playerId] = new PlayerRuntimeData();
        }
    }

    public PlayerRuntimeData GetPlayerData(string playerId)
    {
        return _players.TryGetValue(playerId, out var data) ? data : null;
    }
}