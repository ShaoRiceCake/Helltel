// Controller层核心逻辑
using System;
using Michsky.LSS;
using UnityEngine;

/// <summary>
/// 游戏逻辑控制器（挂载到场景持久化物体）
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("数据引用")]
    public GameDataModel _gameData;
    public LoadingScreenManager lSS_Manager;

    public static GameController Instance { get; private set; }

    private void Awake() {
        if (!Instance) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        _gameData.ResetData();
    }
    
    // public void Update()
    // {
    //     if(Input.GetKeyDown(KeyCode.M))
    //     {
    //         AddPerformance(50);
    //         AudioManager.Instance.Play("压抑氛围环境音",owner:this);
    //         
    //     }
    //     if(Input.GetKeyDown(KeyCode.J))
    //     {
    //         DeductHealth(50);
    //         AudioManager.Instance.Stop("压抑氛围环境音",owner:this);
    //         
    //     }
    // }
    
    // 增加血上限接口
    public void AddMaxHealth(int amount)
    {
        _gameData.MaxHealth += amount;
    }

    // 增加血量接口
    public void AddHealth(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("请使用 DeductHealth 方法扣除血量");
            return;
        }
        _gameData.Health += amount;
    }

    // 扣血接口
    public void DeductHealth(int amount)
    {
        if (_gameData.Health >= amount)
        {
            _gameData.Health -= amount;
            BloodEffectController.ActivateBloodEffect();
            Debug.Log("玩家扣除血量" + amount);
        }
        else
        {
            Debug.Log("死亡");
            return;
        }
    }

    // 增加玩家资金接口
    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("请使用 DeductMoney 方法扣除金钱");
            return;
        }
        
        _gameData.Money += amount;
    }
    
    // 扣玩家资金接口
    public void DeductMoney(int amount)
    {
        _gameData.Money -= amount; 
    }

    // 扣绩效接口
    public void DeductPerformance(int amount)
    {
        _gameData.Performance -= amount;  
    }
    
    // 检查玩家资金接口
    public int GetMoney()
    {
        return _gameData.Money;
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

}