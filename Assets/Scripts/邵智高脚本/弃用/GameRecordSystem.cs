using UnityEngine;
using System;

/// <summary>
/// 游戏全局记录系统（单例模式）
/// 负责管理金钱、绩效、天数、关卡等核心游戏数据
/// 使用事件机制通知数据变化
/// </summary>
public class GameRecordSystem : MonoBehaviour
{
    #region 单例实例
    private static GameRecordSystem _instance;
    
    /// <summary>
    /// 全局访问点
    /// </summary>
    public static GameRecordSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameRecordSystem>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameRecordSystem");
                    _instance = obj.AddComponent<GameRecordSystem>();
                }
            }
            return _instance;
        }
    }
    #endregion

    #region 事件定义
    //这里定义了一堆事件，之后这些信息更新时就会同时所有相关的方法去触发
    //之后可以通过例如recordSystem.OnMoneyChanged += 订阅了事件的方法名;来订阅
    /// <summary> 当金钱变化时触发（参数：新金额）</summary>
    public event Action<int> OnMoneyChanged;
    
    /// <summary> 当日数推进时触发（参数：当前天数）</summary>
    public event Action<int> OnDayPassed;
    
    /// <summary> 绩效检查通过时触发（参数：实际达成金额）</summary>
    public event Action<int> OnPerformancePassed;
    
    /// <summary> 绩效检查失败时触发 </summary>
    public event Action OnPerformanceFailed;
    
    /// <summary> 玩家人数变化时触发（参数：新人数）</summary>
    //public event Action<int> OnPlayerCountChanged;
    
    /// <summary> 关卡变化时触发（参数：新关卡层级）</summary>
    public event Action<int> OnLevelChanged;
    #endregion

    #region 核心数据字段
    [SerializeField, Tooltip("基础绩效增长系数")]
    private float performanceGrowthRate = 1.2f;
    [SerializeField, Tooltip("钱")]
    private int _money; //当前拥有的钱，这里的钱在关卡内不显示出来，只在买东西时显示拥有的钱。钱和绩效是分离的，只有本日赚取的钱才是绩效
    [SerializeField, Tooltip("绩效")]
    public int _performance; //当前的绩效。绩效是本日获得的钱。
    [SerializeField, Tooltip("绩效目标")]
    public int _performanceTarget; //当前绩效目标
    public int _currentLevel = 0; // 当前所在层级（0层为大厅）
    public int _playerCount = 0;  // 当前玩家人数
    public int _gameDay = 1;      // 当前游戏天数
    [SerializeField, Tooltip("当前时间")]
    private TimeSpan _time = new TimeSpan(0, 0, 0);

    // 用于安全的获取数据。例如在别的脚本中用 GameRecordSystem.Instance.GetMoney来获取_money;
    public int GetMoney => _money;//钱
    public int GetPerformance => _performance;//绩效
    public int GetPerformanceTarget => _performanceTarget;//绩效目标
    public int GetLevel => _currentLevel;//当前层级
    public int GetPlayerCount => _playerCount;//玩家数
    public int GetDay => _gameDay;//天数
    public TimeSpan GetTime => _time;//当前时间

    #endregion

    #region 生命周期
    private void Awake()
    {
        // 单例初始化
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeNewGame();
    }

    /// <summary>
    /// 初始化新游戏数据
    /// </summary>
    public void InitializeNewGame()
    {
        _money = 0;
        _performance = 0;
        _performanceTarget = 100; // 初始绩效目标
        _currentLevel = 0;
        
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 增加指定金额（负数视为扣除）
    /// </summary>
    public void ModifyMoney(int amount)
    {
        _money = Mathf.Max(0, _money + amount);
        _performance = Mathf.Max(0, _performance + amount);
        OnMoneyChanged?.Invoke(_money);
    }
    public void SetMoney(int amount)
    { 
        _money = amount;
       
        OnMoneyChanged?.Invoke(_money);
    }


    /// <summary>
    /// 推进游戏天数
    /// </summary>
    public void AdvanceDay()
    {
        _gameDay++;
        //通知所有订阅了OnDayPassed事件的订阅者新的_gameDay
        OnDayPassed?.Invoke(_gameDay);
        
        // 每天进行绩效检查
        CheckPerformance();
    }

    /// <summary>
    /// 更新当前关卡层级
    /// </summary>
    public void UpdateLevel(int newLevel)
    {
        _currentLevel = Mathf.Clamp(newLevel, -18, 0); // 限制层级范围
        //通知所有订阅了OnLevelChanged事件的订阅者新的_currentLevel
        OnLevelChanged?.Invoke(_currentLevel);
    }

    /// <summary>
    /// 更新玩家人数
    /// </summary>
    // public void UpdatePlayerCount(int delta)
    // {
    //     _playerCount = Mathf.Max(0, _playerCount + delta);
    //     OnPlayerCountChanged?.Invoke(_playerCount);
    // }
    #endregion

    #region 绩效逻辑
    private void CheckPerformance()
    {
        if (_performance >= _performanceTarget)
        {
            HandlePerformanceSuccess();
        }
        else
        {
            HandlePerformanceFailure();
        }
    }
    //成功达成绩效目标
    private void HandlePerformanceSuccess()
    {
        // 计算新绩效目标
        _performanceTarget = Mathf.FloorToInt(_performanceTarget * performanceGrowthRate);
        //通知并传递所有订阅了OnPerformancePassed的订阅者并传递
        OnPerformancePassed?.Invoke(_money);
        OnMoneyChanged?.Invoke(_money);
    }

    private void HandlePerformanceFailure()
    {
        // 触发失败事件
        OnPerformanceFailed?.Invoke();
        
        
        OnMoneyChanged?.Invoke(_money);
        
        // 重置到初始状态
        _performanceTarget = 100;
        _gameDay = 1;
        UpdateLevel(0);
    }
    #endregion

    #region 工具方法
    
    
    /// <summary>
    /// 获取到下个绩效检查日的剩余天数   这个方法现在应该没用了
    /// </summary>
    // public int GetDaysUntilCheck()
    // {
    //     return 3 - (_gameDay % 3);
    // }
    #endregion
}