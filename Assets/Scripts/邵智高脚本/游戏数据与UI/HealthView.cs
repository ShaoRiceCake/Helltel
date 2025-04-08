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
        _data.OnPlayerHealthChanged += UpdateHealth;
    }

    private void Start()
    {
        //_playerId = "p1";
        UpdateHealth(_localPlayerId,_data.GetPlayer(_localPlayerId).Health);
    }
    public void BindLocalPlayer(string playerId)
    {
        _localPlayerId = playerId;
        
        // 立即显示初始值
        var playerData = _data.GetPlayer(_localPlayerId);
        if(playerData != null)
            UpdateHealth(_localPlayerId, playerData.Health);
    }
 

    private void UpdateHealth(string id, int health)
    {
        if(id == _localPlayerId)
            _healthText.text = $"HP: {health}";
    }



    private void OnDestroy()
    {
        _data.OnPlayerHealthChanged -= UpdateHealth;
        //_animSequence?.Kill();
    }
}