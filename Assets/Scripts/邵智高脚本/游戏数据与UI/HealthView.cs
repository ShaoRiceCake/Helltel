using UnityEngine;
using TMPro;
using DG.Tweening; 

/// <summary>
/// 使用DOTween的生命值显示视图
/// </summary>
public class HealthView : MonoBehaviour
{
    [Header("组件绑定")]
    [SerializeField] private ProgressBarPro progressBar;
    private GameDataModel _data;
    
    private void Start()
    {
        _data = Resources.Load<GameDataModel>("GameData");
        _data.OnHealthChanged += UpdateHealth;
        UpdateHealth(_data.Health);
        
    }

    private void UpdateHealth(int health)
    {
        progressBar.SetValue(health,100f);
    }
    
    private void OnDestroy()
    {
        _data.OnHealthChanged -= UpdateHealth;
    }
}