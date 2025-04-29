using UnityEngine;
using TMPro;
using DG.Tweening; 

/// <summary>
/// 使用DOTween的生命值显示视图
/// </summary>
public class HealthView : MonoBehaviour
{
    [Header("组件绑定")]
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private GameDataModel _data;
    
    private string _localPlayerId; 

    private void Awake()
    {
        _data.OnHealthChanged += UpdateHealth;
        
        //EventBus<HealthChangedEvent>.Subscribe(OnHealthChanged, this);
    }

    private void Start()
    {
        UpdateHealth(_data.Health);
        
    }
  
 

    private void UpdateHealth(int health)
    {
        _healthText.text = $"{health}";
    }
    
    // private void OnHealthChanged(HealthChangedEvent evt)
    // {
    //     // 更新UI显示
    //     if (_healthText != null)
    //     {
    //         _healthText.text = $"{evt.CurrentHealth}";
    //     }
    // }
    private void OnDestroy()
    {
        _data.OnHealthChanged -= UpdateHealth;
        
        //EventBus<HealthChangedEvent>.UnsubscribeAll(this);

    }
}