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
    [SerializeField] private string _playerId;

    private void Awake()
    {
        _data.OnPlayerHealthChanged += UpdateHealth;
    }

    private void Start()
    {
        _playerId = "p1";
        UpdateHealth(_playerId,_data.GetPlayer(_playerId).Health);
    }

    private void UpdateHealth(string id, int health)
    {
        if(id == _playerId)
            _healthText.text = $"HP: {health}";
    }



    private void OnDestroy()
    {
        _data.OnPlayerHealthChanged -= UpdateHealth;
        //_animSequence?.Kill();
    }
}