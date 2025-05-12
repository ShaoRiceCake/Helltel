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

    private void Start()
    {
        
    }
    private void NeedActive(string sceneName)
    {
        if(sceneName == _data.dungeon)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdatePerformanceDisplay(int newValue)
    {
        _performanceText.text = $"{newValue}";
  
    }
    private void UpdatePerformanceTargetDisplay(int newValue)
    {
        _performanceTargetText.text = $"{newValue}";
    }



    private void OnDestroy()
    {
        _data.OnPerformanceChanged -= UpdatePerformanceDisplay;
        _data.OnPerformanceChanged -= UpdatePerformanceTargetDisplay;
        _data.FloorIS -= NeedActive;

    }
}