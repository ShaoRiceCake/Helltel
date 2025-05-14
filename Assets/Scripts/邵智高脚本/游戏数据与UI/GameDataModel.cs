//Model层核心数据
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// 全局游戏数据模型
[CreateAssetMenu(fileName = "GameData", menuName = "Helltel/Data/GameData")]
public class GameDataModel : ScriptableObject
{

    // 私有字段配合属性保护数据
    [SerializeField]private int _health;
    [SerializeField]private int _maxHealth;
    [SerializeField]private int _money;
    [SerializeField]private int _performance;
    [SerializeField]private int _performanceTarget;
    [SerializeField]private int _basePerformanceTarget ;//基础绩效要求
    [SerializeField]private int _level;
    [SerializeField]private bool _isPerformancePassed;
    [SerializeField]private bool _isPlayerDied ;

    
    //输入商店和地牢的场景名
    [Tooltip("第一个外部场景（Scene1）")]
    public string dungeon = "单机正式地牢";
    
    [Tooltip("第二个外部场景（Scene2）")]
    public string shop = "单机正式商店";
    //当前场景别输入
    [SerializeField]private string _currentLoadedScene;

    // 公开事件
    public event Action<int> OnHealthChanged;      // 生命变化
    public event Action<int> OnMaxHealthChanged;      // 最大生命变化
    public event Action<int> OnMoneyChanged;      // 金钱变化
    public event Action<int> OnPerformanceChanged;// 绩效变化
    public event Action<int> OnPerformanceTargetChanged;// 绩效要求变化
    //public event Action<bool> IsPerformancePassed;  // 绩效是否达标
    //public event Action OnPerformanceFailed;      // 绩效失败
    public event Action<int> OnLevelChanged;      // 关卡变化
    public event Action OnFloorChanged;  //层级变化
    public event Action StartLoading;  //开始加载
    public event Action FinishLoading;  //结束加载
    public event Action<string> FloorIS;      // 楼层类型为
    public event Action<bool> OnIsPlayerDiedChangedEvent;      // 玩家死亡状态变化事件

    // 属性封装（数据访问入口）
    public int Health {
        get => _health;
        set {
            if(value<=0)
            {
                IsPlayerDied = true;
                _health = 0;
            }
            else
            {
                _health =value;
            }
            OnHealthChanged?.Invoke(_health); // 触发UI更新
        }
    }
    public int MaxHealth {
        get => _maxHealth;
        set {
            _maxHealth =value;
            OnMaxHealthChanged?.Invoke(_maxHealth); // 触发UI更新
        }
    }
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
            // 数值变化时自动检查绩效，并传递是否达标的事件
            //IsPerformancePassed?.Invoke(CheckPerformance());

        }
    }

    public int PerformanceTarget {
        get => _performanceTarget;
        set {
             _performanceTarget = value;
            OnPerformanceTargetChanged?.Invoke(_performanceTarget);
          
        }
    }
    public int Level {
        get => _level;
        set {
            _level =value;
            OnLevelChanged?.Invoke(_level); 
        }
    }
    
    public string CurrentLoadedScene {
        get => _currentLoadedScene;
        set {
            _currentLoadedScene = value;
            FloorIS?.Invoke(_currentLoadedScene);
        }
    }
    public bool IsPlayerDied {
        get => _isPlayerDied;
        set {
            _isPlayerDied = true;
            OnIsPlayerDiedChangedEvent?.Invoke(value);
        }
    }

    // 初始化方法
    public void ResetData()
    {
        Money = 0;
        MaxHealth = 100;
        Health = MaxHealth;
        Performance = 0;
        _basePerformanceTarget = 100;
        PerformanceTarget = _basePerformanceTarget;
        Level = 0;
        _isPerformancePassed = false;
        _currentLoadedScene = "";
        IsPlayerDied = false;
    }
    //进商店层调用这个
    public void NewShopFloorData()
    {
        // Money += Performance;
        Health = MaxHealth;
        // Performance = 0;

        //改由商店内部脚本实现
    }
    //进地牢层调用这个
    public void NewDungeonFloorData()
    {
        Health = MaxHealth;
        PerformanceTarget = ExponentialCalculation();
        _isPerformancePassed = false;

    }

    private int ExponentialCalculation()
    {
        // 计算指数增长后的值
        var value = _basePerformanceTarget * Mathf.Pow(1.5f, Level);
        return Mathf.RoundToInt(value / 10f) * 10;
    }
    public bool CheckPerformance()
    {
        if(Performance>=PerformanceTarget)
        {
            _isPerformancePassed = true;
            return true;
        }
        else
        {
            _isPerformancePassed = false;
            return false;
        }
        
    }
    public void SendOnFloorChangedEvent()
    {
        OnFloorChanged?.Invoke(); 
    }
    public void SendStartLoading()
    {
        StartLoading?.Invoke(); 
    }
    public void SendFinishLoading()
    {
        FinishLoading?.Invoke(); 
    }
    
    


}
