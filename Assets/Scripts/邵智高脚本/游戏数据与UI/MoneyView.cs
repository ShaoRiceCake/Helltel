using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// 使用DOTween的金钱显示视图
/// </summary>
public class MoneyView : MonoBehaviour
{
    [Header("组件绑定")]
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private RawImage _moneyImage; // 金币堆的RawImage
    
    [Header("动画设置")]
    [SerializeField] private float _baseScaleDuration = 0.3f; // 基础动画时长
    [SerializeField] private float _maxAdditionalScale = 0.5f; // 最大额外缩放比例
    [SerializeField] private float _scaleMultiplier = 1.2f; // 每次增加的缩放乘数
    [SerializeField] private float _decreaseScaleDuration = 0.5f; // 减少时的动画时长
    
    private GameDataModel _data;
    private int _lastMoneyValue;
    private int _consecutiveChangeCount; // 连续增加/减少的次数
    private Vector3 _originalScale; // 原始大小
    private Sequence _currentSequence; // 当前动画序列

    private void Awake()
    {
        _data = Resources.Load<GameDataModel>("GameData");
        _originalScale = _moneyImage.transform.localScale;
        _lastMoneyValue = _data.Money;
        
        // 注册事件
        _data.OnMoneyChanged += UpdateMoneyDisplay;
        UpdateMoneyDisplay(_data.Money);
    }

    private void UpdateMoneyDisplay(int newValue)
    {
        _moneyText.text = $"{newValue}";
        
        // 计算变化量
        var change = newValue - _lastMoneyValue;
        
        // 如果有变化，执行动画
        if (change != 0)
        {
            if (_currentSequence != null && _currentSequence.IsActive())
            {
                _currentSequence.Kill();
            }
            
            if (change > 0)
            {
                HandleMoneyIncrease(change);
            }
            else
            {
                HandleMoneyDecrease();
            }
        }
        
        _lastMoneyValue = newValue;
    }

    private void HandleMoneyIncrease(int change)
    {
        // 如果是连续增加，增加计数，否则重置
        if (_consecutiveChangeCount >= 0)
        {
            _consecutiveChangeCount++;
        }
        else
        {
            _consecutiveChangeCount = 1;
        }
        
        // 计算额外缩放比例（有上限）
        var additionalScale = Mathf.Min(_maxAdditionalScale, 
            _consecutiveChangeCount * 0.05f * _scaleMultiplier);
        
        // 创建动画序列
        _currentSequence = DOTween.Sequence();
        
        // 猛地变大
        _currentSequence.Append(_moneyImage.transform.DOScale(
            _originalScale * (1.2f + additionalScale), 
            _baseScaleDuration * 0.7f).SetEase(Ease.OutQuad));
        
        // 然后缩小回原始大小，带有一点弹性效果
        _currentSequence.Append(_moneyImage.transform.DOScale(
            _originalScale, 
            _baseScaleDuration * 1.3f).SetEase(Ease.OutElastic));
    }

    private void HandleMoneyDecrease()
    {
        // 如果是连续减少，增加计数，否则重置
        if (_consecutiveChangeCount <= 0)
        {
            _consecutiveChangeCount--;
        }
        else
        {
            _consecutiveChangeCount = -1;
        }
        
        // 减少时的动画效果 - 直接缩小，没有额外效果
        _moneyImage.transform.DOScale(
            _originalScale * 0.9f, 
            _decreaseScaleDuration * 0.5f).SetEase(Ease.OutQuad)
            .OnComplete(() => {
                _moneyImage.transform.DOScale(
                    _originalScale, 
                    _decreaseScaleDuration * 0.5f).SetEase(Ease.InOutQuad);
            });
    }

    private void OnDestroy()
    {
        _data.OnMoneyChanged -= UpdateMoneyDisplay;
        
        if (_currentSequence != null && _currentSequence.IsActive())
        {
            _currentSequence.Kill();
        }
    }
}