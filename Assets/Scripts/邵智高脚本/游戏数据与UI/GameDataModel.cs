//Model层核心数据
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// 全局游戏数据模型
[CreateAssetMenu(fileName = "GameData", menuName = "Helltel/Data/GameData")]
public class GameDataModel : ScriptableObject
{
    [Header("绩效增长倍率参数")]
    [SerializeField, Range(1.1f, 2f)] private float _performanceGrowth = 1.2f;
    [SerializeField] private int _baseTarget = 100;//基础绩效要求

    // 私有字段配合属性保护数据
    private int _money;
    private int _performance;
    private int _day = 1;
    private int _level = -1;
    private Dictionary<string, PlayerData> _players = new();
    

    // 公开事件
    public event Action<int> OnMoneyChanged;      // 金钱变化
    public event Action<int> OnPerformanceChanged;// 绩效变化
    public event Action<int> OnPerformanceTargetChanged;// 绩效要求变化
    public event Action<int> OnDayChanged;        // 天数变化
    public event Action<int> OnPerformancePassed;  // 绩效达标 
    public event Action OnPerformanceFailed;      // 绩效失败
    public event Action<int> OnLevelChanged;      // 层级变化
    public event Action<string, int> OnPlayerHealthChanged; // 玩家ID, 新生命值

    // 属性封装（数据访问入口）
    public int Money {
        get => _money;
        set {
            _money =value;
            OnMoneyChanged?.Invoke(_money); // 触发UI更新
        }
    }

    public int Performance {
        get => _performance;
        set {
            _performance = value;
            OnPerformanceChanged?.Invoke(_performance);
            //CheckPerformance(); // 数值变化时自动检查绩效
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
    public int PlayerCount => _players.Count;

    // 初始化方法
    public void ResetData()
    {
        Money = 0;
        Performance = 0;
        PerformanceTarget = _baseTarget;
        CurrentDay = 1;
        _level = -1;
    }

    // 内部绩效检查
    // private void CheckPerformance()
    // {
    //     if (Performance >= PerformanceTarget)
    //     {
    //         PerformanceTarget = Mathf.FloorToInt(PerformanceTarget * _performanceGrowth);
    //         OnPerformancePassed?.Invoke(PerformanceTarget);
    //     }
    //     else 
    //     {
    //         OnPerformanceFailed?.Invoke();
    //     }
    // }
    //======= 玩家数据类 =======//
    public class PlayerData
    {
        private int _health = 100;
        private int _maxHealth = 100;
        public int Health
        {
            get => _health;
            set
            {
                int newVal = Mathf.Clamp(value, 0, _maxHealth);
                if (newVal != _health)
                {
                    _health = newVal;
                    OnHealthChanged?.Invoke(newVal);
                }
            }
        }

        public event Action<int> OnHealthChanged;
    }

    //======= 玩家管理接口 =======//
    //所有玩家加入游戏后，游戏开始要为每个玩家注册，来储存对应ID的生命值
    // public void RegisterNetworkPlayer(string id)
    // {
    //     if (!_players.ContainsKey(id))
    //     {
    //         var player = new PlayerData();
    //         // 给玩家的OnHealthChanged添加监听,当OnHealthChanged触发时触发另一个叫OnPlayerHealthChanged的事件，该事件会将玩家AI和生命值传递出去
    //         player.OnHealthChanged += (health) => 
    //             OnPlayerHealthChanged?.Invoke(id, health);
    //         //将新玩家存入字典
    //         _players[id] = player;
    //         // 触发初始值显示
    //         player.Health = player.Health; // 强制触发事件
    //     }
    //     // 如果已经有这个ID，什么都不做（避免重复注册）
    // }
    // /// <summary>
    // /// 根据ID获取玩家数据（找不到返回null）
    // /// </summary>
    // public PlayerData GetPlayer(string id) => _players.TryGetValue(id, out var p) ? p : null;
}
