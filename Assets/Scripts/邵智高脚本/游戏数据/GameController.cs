// Controller层核心逻辑
using UnityEngine;

/// <summary>
/// 游戏逻辑控制器（挂载到场景持久化物体）
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("数据引用")]
    [SerializeField] private GameDataModel _gameData;

    // 依赖注入初始化
    private void Awake()
    {
        // 持久化单例模式
        if (FindObjectsOfType<GameController>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        _gameData.ResetData();
    }

    // 外部接口：修改金钱
    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("请使用 DeductMoney 方法扣除金钱");
            return;
        }
        
        _gameData.Money += amount;
        _gameData.Performance += amount; // 绩效同步增加
    }

    // 安全扣钱方法
    public bool TryDeductMoney(int amount)
    {
        if (_gameData.Money >= amount)
        {
            _gameData.Money -= amount;
            return true;
        }
        return false;
    }

    // 推进天数（原AdvanceDay）
    public void AdvanceDay()
    {
        _gameData.CurrentDay++;
        // 自动绩效检查由Model内部处理
    }
}