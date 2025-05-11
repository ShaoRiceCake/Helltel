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


    private void Awake()
    {
       
        
        
    }

    private void Start()
    {
        _data = Resources.Load<GameDataModel>("GameData");
        // 注册事件
        _data.OnMoneyChanged += UpdateMoneyDisplay;
        _data.FloorIS +=NeedActive;
        UpdateMoneyDisplay(_data.Money);
    }
    private void NeedActive(string sceneName)
    {
        if(sceneName == _data.shop)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdateMoneyDisplay(int newValue)
    {
        _moneyText.text = $"{newValue}";
    
    }



    private void OnDestroy()
    {
        _data.OnMoneyChanged -= UpdateMoneyDisplay;
        _data.FloorIS -= NeedActive;
    }
}