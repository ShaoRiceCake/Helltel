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
}