using UnityEngine;
using System.Collections;

public class ShopTrigger : MonoBehaviour
{
    public bool isShopActive;
    private bool _hasTriggered = false; // 标记是否已经触发过
    
    [Header("金币发射器")]
    public GameObject externalObject; // 需要控制激活状态的外部物体
    
    [Header("音效设置")]
    public float minSoundInterval = 0.1f; // 最小音效间隔
    public float maxSoundInterval = 0.8f; // 最大音效间隔
    public int soundThresholdLow = 10; // 低变化阈值
    public int soundThresholdHigh = 20; // 高变化阈值
    
    [Header("转换时长控制")]
    public float totalDuration = 10f;
    
    private Coroutine _performanceConversionCoroutine;
    private float _currentSoundInterval;
    private float _lastSoundTime;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered || !other.CompareTag("Player")) return;

        isShopActive = true;
        _hasTriggered = true;
        
        // 开始绩效转换逻辑
        StartPerformanceConversion();
    }

    private void StartPerformanceConversion()
    {
        if (_performanceConversionCoroutine != null)
        {
            StopCoroutine(_performanceConversionCoroutine);
        }
        
        _performanceConversionCoroutine = StartCoroutine(ConvertPerformanceToMoney());
    }

    private IEnumerator ConvertPerformanceToMoney()
    {
        var gameData = GameController.Instance._gameData;
        var remainingPerformance = gameData.Performance;
    
        if (remainingPerformance <= 0)
        {
            yield break;
        }
    
        // 配置参数
        const float conversionRate = 10f; // 每秒转换10金币
        const float updateInterval = 0.1f; // 每0.1秒更新一次
    
        // 激活外部物体
        if (externalObject)
        {
            externalObject.SetActive(true);
        }
    
        // 计算初始音效间隔
        _currentSoundInterval = 0.2f;
    
        // 开始转换
        float accumulatedTime = 0f;
        while (remainingPerformance > 0)
        {
            yield return new WaitForSeconds(updateInterval);
            accumulatedTime += updateInterval;
        
            // 计算本次应该转换的数量
            float desiredConversion = conversionRate * accumulatedTime;
            int actualAmount = Mathf.FloorToInt(desiredConversion);
        
            if (actualAmount <= 0) continue;
        
            // 确保不超过剩余绩效
            actualAmount = Mathf.Min(actualAmount, remainingPerformance);
        
            // 执行转换
            GameController.Instance.DeductPerformance(actualAmount);
            GameController.Instance.AddMoney(actualAmount);
            remainingPerformance -= actualAmount;
        
            // 重置累计
            accumulatedTime = 0f;
        
            // 播放音效
            PlayConversionSound();
        }
    
        // 转换完成，失活外部物体
        if (externalObject)
        {
            externalObject.SetActive(false);
        }
    
        _performanceConversionCoroutine = null;
    }


    private void PlayConversionSound()
    {
        if (!(Time.time - _lastSoundTime >= _currentSoundInterval)) return;
        AudioManager.Instance.Play("金币掉落");
        _lastSoundTime = Time.time;
    }

    private void OnDisable()
    {
        if (_performanceConversionCoroutine == null) return;
        StopCoroutine(_performanceConversionCoroutine);
        _performanceConversionCoroutine = null;
    }
}