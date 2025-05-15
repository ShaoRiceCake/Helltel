using System;
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
    [SerializeField] private ProgressBarPro progressBar;

    private int _currentValue;
    private int _currentTargetValue;

    public int CurrentValue
    {
        get => _currentValue;
        set
        {
            _currentValue = value;
            progressBar.SetValue(_currentValue,_currentTargetValue);
        }
    }
    
    public int CurrentTargetValue
    {
        get => _currentTargetValue;
        set
        {
            _currentTargetValue = value;
            progressBar.SetValue(_currentValue,_currentTargetValue);
        }
    }

    
    private GameDataModel _data;
    private void Awake()
    {
        _data = Resources.Load<GameDataModel>("GameData");
        _data.OnPerformanceChanged += UpdatePerformanceDisplay;
        _data.OnPerformanceTargetChanged += UpdatePerformanceTargetDisplay;
        _data.FloorIS +=NeedActive;
        UpdatePerformanceDisplay(_data.Performance);
        UpdatePerformanceTargetDisplay(_data.PerformanceTarget);
        
    }

    private void NeedActive(string sceneName)
    {
        gameObject.SetActive(sceneName == _data.dungeon);
    }

    private void UpdatePerformanceDisplay(int newValue)
    {
        _performanceText.text = $"{newValue}";
        CurrentValue =  newValue;
    }
    private void UpdatePerformanceTargetDisplay(int newValue)
    {
        _performanceTargetText.text = $"{newValue}";
        CurrentTargetValue = newValue;
    }
    
    private void OnDestroy()
    {
        _data.OnPerformanceChanged -= UpdatePerformanceDisplay;
        _data.OnPerformanceChanged -= UpdatePerformanceTargetDisplay;
        _data.FloorIS -= NeedActive;

    }
}