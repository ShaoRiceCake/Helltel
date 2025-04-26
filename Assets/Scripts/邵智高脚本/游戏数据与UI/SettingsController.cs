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
    public Button btn_BackOfSettingPanel; //返回按钮
    [Header("各设置界面")]
    public GameObject panelGame;   //游戏面板
    public GameObject panelQuality;   //游戏面板
    public GameObject panelAuido;   //游戏面板
    public GameObject panelControlls;   //游戏面板

    [Header("音量控制界面")]
    
    [SerializeField] private Button btnAudioResetToDefault;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private Toggle toggle3D;
    [SerializeField] private Toggle toggleReverb;
 
    
  



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
        btn_BackOfSettingPanel.onClick.AddListener(globalUIController.CloseSettings);
        //初始化
        InitializeSettings();
        //初始话音频设置
        InitializeAduioDefaultValues();
        SetupRealTimeUpdates();
        
        
    }
    /// <summary>
    /// 初始化默认音频设置（无需加载保存的数据）
    /// </summary>
    private void InitializeAduioDefaultValues()
    {
        // 主音量
        masterSlider.value = 1f;
        AudioManager.Instance.SetMasterVolume(masterSlider.value);

        // 音乐音量
        musicSlider.value = 0.7f;
        AudioManager.Instance.SetCategoryVolume(AudioCategory.Music, musicSlider.value);

        // 音效音量
        sfxSlider.value = 0.7f;
        AudioManager.Instance.SetCategoryVolume(AudioCategory.SFX, sfxSlider.value);

        // 语音音量
        voiceSlider.value = 0.7f;
        AudioManager.Instance.SetCategoryVolume(AudioCategory.Voice, voiceSlider.value);

        // // 3D音频（默认启用）
        // toggle3D.isOn = true;
        // AudioManager.Instance.Toggle3DAudio(true);

        // 混响效果（默认启用）
        // toggleReverb.isOn = true;
        // AudioManager.Instance.ToggleReverb(true);
    }

    /// <summary>
    /// 设置实时更新（数值改变立即生效）
    /// </summary>
    private void SetupRealTimeUpdates()
    {
        // 主音量
        masterSlider.onValueChanged.AddListener(v => {
            AudioManager.Instance.SetMasterVolume(v);
        });

        // 音乐音量
        musicSlider.onValueChanged.AddListener(v => {
            AudioManager.Instance.SetCategoryVolume(AudioCategory.Music, v);
        });

        // 音效音量
        sfxSlider.onValueChanged.AddListener(v => {
            AudioManager.Instance.SetCategoryVolume(AudioCategory.SFX, v);
        });

        // 语音音量
        voiceSlider.onValueChanged.AddListener(v => {
            AudioManager.Instance.SetCategoryVolume(AudioCategory.Voice, v);
        });
        //重置
        btnAudioResetToDefault.onClick.AddListener(ResetToDefault);

        // // 3D音频开关
        // toggle3D.onValueChanged.AddListener(v => {
        //     AudioManager.Instance.Toggle3DAudio(v);
        // });

        // // 混响开关
        // toggleReverb.onValueChanged.AddListener(v => {
        //     AudioManager.Instance.ToggleReverb(v);
        // });
    }

    /// <summary>
    /// 重置为默认设置
    /// </summary>
    public void ResetToDefault()
    {
        // 重置滑动条值（自动触发事件）
        masterSlider.value = 1f;
        musicSlider.value = 0.7f;
        sfxSlider.value = 0.7f;
        voiceSlider.value = 0.7f;
        toggle3D.isOn = true;
        toggleReverb.isOn = true;
        AudioManager.Instance.Play("泡泡音");
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
        AudioManager.Instance.Play("泡泡音");
    }
    //打开图形面板
    private void OpenQualityPanel()
    {
        CloseAllPanels();
        panelQuality.gameObject.SetActive(true);
        AudioManager.Instance.Play("泡泡音");
    }
    //打开音频面板
    private void OpenAudioPanel()
    {
        CloseAllPanels();
        panelAuido.gameObject.SetActive(true);
        AudioManager.Instance.Play("泡泡音");
    }
    //打开控制面板
    private void OpenControllPanel()
    {
        CloseAllPanels();
        panelControlls.gameObject.SetActive(true);
        AudioManager.Instance.Play("泡泡音");
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