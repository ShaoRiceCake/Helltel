using UnityEngine;
using TMPro;
using DG.Tweening; 

/// <summary>
/// 使用DOTween的金钱显示视图
/// </summary>
public class MoneyView : MonoBehaviour
{
    [Header("组件绑定")]
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private GameDataModel _data;

    // [Header("动画参数")]
    // [SerializeField] private float _punchScale = 1.2f;
    // [SerializeField] private float _animDuration = 0.3f;
    // [SerializeField] private Color _flashColor = Color.yellow;

    private Sequence _animSequence;

    private void Awake()
    {
       
        
        // 注册事件
        _data.OnMoneyChanged += UpdateMoneyDisplay;
    }

    private void Start()
    {
        UpdateMoneyDisplay(_data.Money);
    }

    private void UpdateMoneyDisplay(int newValue)
    {
        _moneyText.text = $"${newValue}";
        //PlayMoneyAnimation();
    }

    // private void PlayMoneyAnimation()
    // {
    //     // 停止已有动画防止重叠
    //     _animSequence?.Kill();

    //     // 创建动画序列
    //     _animSequence = DOTween.Sequence()
    //         .Append(_moneyText.transform.DOScale(_originalScale * _punchScale, _animDuration/2))
    //         .Join(_moneyText.DOColor(_flashColor, _animDuration/4))
    //         .Append(_moneyText.transform.DOScale(_originalScale, _animDuration/2))
    //         .Join(_moneyText.DOColor(_originalColor, _animDuration/2))
    //         .SetEase(Ease.OutBack);
    // }

    private void OnDestroy()
    {
        _data.OnMoneyChanged -= UpdateMoneyDisplay;
        _animSequence?.Kill();
    }
}