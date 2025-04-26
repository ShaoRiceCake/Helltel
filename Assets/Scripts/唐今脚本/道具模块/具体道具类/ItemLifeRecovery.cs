using UnityEngine;
using System.Collections;

public class ItemLifeRecovery : ActiveItem
{
    [Header("Recovery Settings")]
    [SerializeField] private float useDuration = 2f; // 总使用时间
    [SerializeField] private int recoveryAmount = 1; // 总恢复量
    [SerializeField] private HealthSystem targetHealthSystem; // 外部健康系统
    
    private Vector3 originalScale;
    private Coroutine useCoroutine;

    protected override void Awake()
    {
        base.Awake();
        originalScale = transform.localScale;
        
        OnUseStart.AddListener(StartUseProcess);
        OnUseEnd.AddListener(() =>
        {
            if (useCoroutine == null) return;
            StopCoroutine(useCoroutine);
            useCoroutine = null;
        });
    }

    private void StartUseProcess()
    {
        Debug.Log($"{itemName} 开始使用");
        useCoroutine = StartCoroutine(UseCoroutine());
    }

    private IEnumerator UseCoroutine()
    {
        var elapsedTime = 0f;
        float interval = 0.1f; // 每0.1秒恢复1点
        float nextRecoveryTime = 0f;

        while (elapsedTime < useDuration)
        {
            elapsedTime += Time.deltaTime;
            var progress = elapsedTime / useDuration;
        
            // 逐渐缩小物体
            transform.localScale = originalScale * (1 - progress * 0.9f);
        
            // 按固定间隔恢复生命值
            if (elapsedTime >= nextRecoveryTime)
            {
                targetHealthSystem.ChangeHealth(-recoveryAmount);
                nextRecoveryTime += interval;
                yield return new WaitForSeconds(interval); // 添加间隔
            }
            else
            {
                yield return null;
            }
        }
    
        CompleteUse();
    }
    
    private void CompleteUse()
    {
        Debug.Log($"{itemName} 使用完成");
    }

    protected override void ExecuteUse()
    {
        // 确保缩放重置（如果需要复用对象而不是销毁，可以取消注释）
        // transform.localScale = originalScale;
        base.ExecuteUse(); // 调用基类销毁方法
    }
}