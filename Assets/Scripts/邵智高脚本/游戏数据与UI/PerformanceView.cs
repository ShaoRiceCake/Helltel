using UnityEngine;
using TMPro;
using DG.Tweening; 

/// <summary>
/// 使用DOTween的金钱显示视图
/// </summary>
public class PerformanceView : MonoBehaviour
{
    [Header("组件绑定")]
    [SerializeField] private TMP_Text _performanceText;
    [SerializeField] private TMP_Text _performanceTargetText;
    [SerializeField] private GameDataModel _data;


    private void Awake()
    {
        // 注册事件
        _data.OnPerformanceChanged += UpdatePerformanceDisplay;
        _data.OnPerformanceTargetChanged += UpdatePerformanceTargetDisplay;
    }

    private void Start()
    {
        UpdatePerformanceDisplay(_data.Performance);
    }

    private void UpdatePerformanceDisplay(int newValue)
    {
        _performanceText.text = $"${newValue}";
        //PlayMoneyAnimation();
    }
    private void UpdatePerformanceTargetDisplay(int newValue)
    {
        _performanceTargetText.text = $"${newValue}";
    }



    private void OnDestroy()
    {
        _data.OnPerformanceChanged -= UpdatePerformanceDisplay;

    }
}