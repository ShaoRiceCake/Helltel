// Controller层核心逻辑
using UnityEngine;

/// <summary>
/// 游戏逻辑控制器（挂载到场景持久化物体）
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("数据引用")]
    [SerializeField] private GameDataModel _gameData;

    public static GameController Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        _gameData.ResetData();
        // 仅供测试，实际由网络模块调用
        // _gameData.RegisterNetworkPlayer("test_local");
        // NotifyLocalPlayerReady("test_local");
    }
    private void OnEnable() {
        
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
    // 推进天数
    public void AdvanceDay()
    {
        _gameData.CurrentDay++;
        
    }
 
    //供网络模块调用
    // public void NotifyLocalPlayerReady(string playerId)
    // {
    //     // 获取视图组件并绑定
    //     var healthView = FindObjectOfType<HealthView>();
    //     if(healthView != null)
    //         healthView.BindLocalPlayer(playerId);
    // }

    //离开服务层
    public void LeaveServiceFloor()
    {
        //这里要补上关门逻辑
        //所有不在电梯里的人被遗弃了，扣血致死
        //要补上进入加载界面的逻辑
        //删除所有玩家
        //删除所有本服务层的逻辑
        ClearUnsavedFloorGameObject();
        //将绩效转换成钱的逻辑
       
    }
    //进入商店
    public void GoToShop()
    {
        //在电梯外生成商店
        //在电梯里生成所有玩家
        //加载界面结束
        //给所有玩家控制权
        //电梯门打开
    }
    //离开商店
    public void LeaveShop()
    {
        //这里要补上关门逻辑
        //所有不在电梯里的人被遗弃了，扣血致死
        //进入加载界面
        //删除所有玩家
        //删除商店

    }
    //进入新的服务层
    public void GoToServiceFloor()
    {
        
    }
    //删除本层各种无需保存的东西
    public void ClearUnsavedFloorGameObject()
    {

    }
}