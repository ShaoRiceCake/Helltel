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
    
    
  

    [Header("图形设置")]
    public TMP_Dropdown qualityDropdown;

    private void Awake()
    {
        globalUIController = GlobalUIController.Instance.GetComponent<GlobalUIController>();
    }
    private void Start()
    {
        
        // 绑定按钮事件
        btnGame.onClick.AddListener(OpenGamePanel);
        // btnQuality.onClick.AddListener(OpenSettings);
        // btnAuido.onClick.AddListener(OpenGuestBook);
        // btnControlls.onClick.AddListener(KillMe);
        btnBack.onClick.AddListener(Back);
        
        
    }
    //打开游戏面板
    private void OpenGamePanel()
    {
        //globalUIController.TogglePause(false);
        //globalUIController.CloseAllGlobalUI();
    }
    //打开图形面板
    private void OpenQualityPanel()
    {
        //globalUIController.TogglePause(false);
        //globalUIController.CloseAllGlobalUI();
    }
    //打开音频面板
    private void OpenAudioPanel()
    {
        //globalUIController.TogglePause(false);
        //globalUIController.CloseAllGlobalUI();
    }
    //打开控制面板
    private void OpenControllPanel()
    {
        //globalUIController.TogglePause(false);
        //globalUIController.CloseAllGlobalUI();
    }
    private void Back()
    {
        globalUIController.CloseAllGlobalUI();
        globalUIController.Menu.gameObject.SetActive(true);
    }

    private void InitializeSettings()
    {
        
        
     
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