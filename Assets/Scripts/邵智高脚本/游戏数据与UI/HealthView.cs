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
    [SerializeField] private Color infoColor = Color.red;

    
    private void Start()
    {
        _data = Resources.Load<GameDataModel>("GameData");
        _data.OnHealthChanged += UpdateHealth;
        _data.OnMaxHealthChanged += UpdateMaxHealth;

        UpdateHealth(_data.Health);
        progressBar.SetBarColor(infoColor);
    }

    private void UpdateHealth(int health)
    {
        progressBar.SetValue(health,100);
    }
    
    private void UpdateMaxHealth(int maxHealth)
    {
        var darkerColor = new Color(
            Mathf.Clamp01(infoColor.r * 0.9f), // 减少10%
            Mathf.Clamp01(infoColor.g * 0.9f),
            Mathf.Clamp01(infoColor.b * 0.9f),
            infoColor.a // 保持透明度不变
        );
        
        infoColor =  darkerColor;
        
        progressBar.SetBarColor(darkerColor);
    }
    
    private void OnDestroy()
    {
        _data.OnHealthChanged -= UpdateHealth;
        _data.OnMaxHealthChanged -= UpdateMaxHealth;

    }
}