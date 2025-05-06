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
    private GameDataModel _data;
    

    private void Awake()
    {
        
        
        //EventBus<HealthChangedEvent>.Subscribe(OnHealthChanged, this);
    }

    private void Start()
    {
        _data = GameController.Instance._gameData;
        _data.OnHealthChanged += UpdateHealth;
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