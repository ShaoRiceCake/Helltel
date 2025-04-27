using UnityEngine;
using System.Collections;

public class ItemLifeRecovery : ActiveItem
{
    [Header("Recovery Settings")]
    [SerializeField] private float useDuration = 2f; // 总使用时间
    [SerializeField] private int recoveryAmount = 1; 
    [SerializeField] private HealthSystem targetHealthSystem;
    
    private Vector3 _originalScale;
    private Coroutine _useCoroutine;

    protected override void Awake()
    {
        base.Awake();
        _originalScale = transform.localScale;
        
        OnUseStart.AddListener(StartUseProcess);
        OnUseEnd.AddListener(() =>
        {
            if (_useCoroutine == null) return;
            StopCoroutine(_useCoroutine);
            _useCoroutine = null;
        });
    }

    private void StartUseProcess()
    {
        Debug.Log($"{itemName} 开始使用");
        _useCoroutine = StartCoroutine(UseCoroutine());
    }

    private IEnumerator UseCoroutine()
    {
        var elapsedTime = 0f;
        const float interval = 0.1f; // 每0.1秒恢复1点
        var nextRecoveryTime = 0f;

        while (elapsedTime < useDuration)
        {
            elapsedTime += Time.deltaTime;
            var progress = elapsedTime / useDuration;
        
            // 逐渐缩小物体
            transform.localScale = _originalScale * (1 - progress * 0.9f);
        
            // 按固定间隔恢复生命值
            if (elapsedTime >= nextRecoveryTime)
            {
                targetHealthSystem.ChangeHealth(-recoveryAmount);
                nextRecoveryTime += interval;
                yield return new WaitForSeconds(interval); 
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
    
}