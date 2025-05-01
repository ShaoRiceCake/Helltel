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
    private int _health;
    private int _maxHealth;
    private int _money;
    private int _performance;
    private int _performanceTarget;//基础绩效要求
    private int _level = 0;
    
    //需要输入场景名
    [Tooltip("第一个外部场景（Scene1）")]
    public string dungeon = "单机正式地牢";
    
    [Tooltip("第二个外部场景（Scene2）")]
    public string shop = "单机正式商店";
    public string _currentLoadedScene ="";

    // 公开事件
    public event Action<int> OnHealthChanged;      // 生命变化
    public event Action<int> OnMaxHealthChanged;      // 最大生命变化
    public event Action<int> OnMoneyChanged;      // 金钱变化
    public event Action<int> OnPerformanceChanged;// 绩效变化
    public event Action<int> OnPerformanceTargetChanged;// 绩效要求变化
    public event Action<int> OnPerformancePassed;  // 绩效达标 
    public event Action OnPerformanceFailed;      // 绩效失败
    public event Action<int> OnLevelChanged;      // 层级变化
    public event Action<string> FloorIS;      // 楼层类型为
    


    // 属性封装（数据访问入口）
    public int Health {
        get => _health;
        set {
            _health =value;
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
            //CheckPerformance(); // 数值变化时自动检查绩效
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




    // 初始化方法
    public void ResetData()
    {
        Money = 0;
        MaxHealth = 100;
        Health = MaxHealth;
        Performance = 0;
        PerformanceTarget = 100;
        _level = 0;
    }


}
