using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    private GlobalUIController globalUIController;

    [Header("设置界面按钮")]
    public Button btnGame;         //游戏设置按钮
    public Button btnQuality;      //图形设置按钮
    public Button btnAuido;        //音频设置按钮
    public Button btnControlls;    //控制设置按钮
    public Button btnBack;         //返回按钮
    [Header("各设置界面")]
    public GameObject panelGame;   //游戏面板
    public GameObject panelQuality;   //游戏面板
    public GameObject panelAuido;   //游戏面板
    public GameObject panelControlls;   //游戏面板
    
  



    private void Awake()
    {
        
    }
    private void Start()
    {
        globalUIController = GlobalUIController.Instance.GetComponent<GlobalUIController>();

        // 绑定按钮事件
        btnGame.onClick.AddListener(OpenGamePanel);
        btnQuality.onClick.AddListener(OpenQualityPanel);
        btnAuido.onClick.AddListener(OpenAudioPanel);
        btnControlls.onClick.AddListener(OpenControllPanel);
        btnBack.onClick.AddListener(globalUIController.OpenMenu);
        //初始化
        InitializeSettings();
        
        
    }
    private void CloseAllPanels()
    {
        panelGame.gameObject.SetActive(false);
        panelQuality.gameObject.SetActive(false);
        panelAuido.gameObject.SetActive(false);
        panelControlls.gameObject.SetActive(false);
    }
    //打开游戏面板
    private void OpenGamePanel()
    {
        CloseAllPanels();
        panelGame.gameObject.SetActive(true);
    }
    //打开图形面板
    private void OpenQualityPanel()
    {
        CloseAllPanels();
        panelQuality.gameObject.SetActive(true);
    }
    //打开音频面板
    private void OpenAudioPanel()
    {
        CloseAllPanels();
        panelAuido.gameObject.SetActive(true);
    }
    //打开控制面板
    private void OpenControllPanel()
    {
        CloseAllPanels();
        panelControlls.gameObject.SetActive(true);
    }
   

    private void InitializeSettings()
    {
        OpenGamePanel();
    }



    // 应用音频设置
    public void ApplyAudioSettings()
    {
        
    }

    // 应用图形设置
    public void ApplyGraphicsSettings()
    {
        
    }
}