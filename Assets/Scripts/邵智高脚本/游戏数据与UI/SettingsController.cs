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
    public Button btn_CloseSettingPanel; //返回按钮
    [Header("各设置界面")]
    public GameObject panelGame;   //游戏面板
    public GameObject panelQuality;   //游戏面板
    public GameObject panelAuido;   //游戏面板
    public GameObject panelControlls;   //游戏面板

    [Header("音量控制界面")]
    
    [SerializeField] private Button btn_AudioResetToDefault;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider voiceSlider;

    [SerializeField] private TextMeshProUGUI masterValueText;
    [SerializeField] private TextMeshProUGUI musicValueText;
    [SerializeField] private TextMeshProUGUI sfxValueText;
    [SerializeField] private TextMeshProUGUI voiceValueText;
    [SerializeField] private Toggle toggleReverb;
 
    
  



    private void Awake()
    {
        
    }
    private void Start()
    {
        globalUIController = GlobalUIController.Instance.GetComponent<GlobalUIController>();

        // 绑定按钮事件
        btnGame.onClick.AddListener(() => OpenPanel(panelGame));
        btnQuality.onClick.AddListener(() => OpenPanel(panelQuality));
        btnAuido.onClick.AddListener(() => OpenPanel(panelAuido));
        btnControlls.onClick.AddListener(() => OpenPanel(panelControlls));
        btn_CloseSettingPanel.onClick.AddListener(globalUIController.CloseSettings);

        //初始话音频设置
        InitializeAduioDefaultValues(false);
        SetupRealTimeUpdates();
        
        
    }
    /// <summary>
    /// 初始化默认音频设置（无需加载保存的数据）
    /// </summary>
    private void InitializeAduioDefaultValues(bool playSFX)
    {
        // 主音量
        masterSlider.value = 70;
        AudioManager.Instance.SetMasterVolume(masterSlider.value);
        masterValueText.text = masterSlider.value.ToString();
        
        

        // 音乐音量
        musicSlider.value = 100;
        AudioManager.Instance.SetCategoryVolume(AudioCategory.Music, musicSlider.value);
        musicValueText.text = musicSlider.value.ToString();
        

        // 音效音量
        sfxSlider.value = 100;
        AudioManager.Instance.SetCategoryVolume(AudioCategory.SFX, sfxSlider.value);
        sfxValueText.text = sfxSlider.value.ToString();
        

        // 语音音量
        voiceSlider.value = 100;
        AudioManager.Instance.SetCategoryVolume(AudioCategory.Voice, voiceSlider.value);
        voiceValueText.text = voiceSlider.value.ToString();
      

        // 混响效果（默认启用）
        // toggleReverb.isOn = true;
        // AudioManager.Instance.ToggleReverb(true);
        if(playSFX)
        {
            AudioManager.Instance.Play("泡泡音");
        }
        
    }

    /// <summary>
    /// 设置实时更新（数值改变立即生效）
    /// </summary>
    private void SetupRealTimeUpdates()
    {
        // 主音量
        masterSlider.onValueChanged.AddListener(v => {
            AudioManager.Instance.SetMasterVolume(v*0.01f);
            masterValueText.text = masterSlider.value.ToString();
        });

        // 音乐音量
        musicSlider.onValueChanged.AddListener(v => {
            AudioManager.Instance.SetCategoryVolume(AudioCategory.Music, v*0.01f);
            musicValueText.text = musicSlider.value.ToString();
        });

        // 音效音量
        sfxSlider.onValueChanged.AddListener(v => {
            AudioManager.Instance.SetCategoryVolume(AudioCategory.SFX, v*0.01f);
            sfxValueText.text = sfxSlider.value.ToString();
        });

        // 语音音量
        voiceSlider.onValueChanged.AddListener(v => {
            AudioManager.Instance.SetCategoryVolume(AudioCategory.Voice, v*0.01f);
            voiceValueText.text = voiceSlider.value.ToString();
        });
        //重置
        btn_AudioResetToDefault.onClick.AddListener( () => InitializeAduioDefaultValues(true));



        // // 混响开关
        // toggleReverb.onValueChanged.AddListener(v => {
        //     AudioManager.Instance.ToggleReverb(v);
        // });
    }


    private void CloseAllPanels()
    {
        panelGame.gameObject.SetActive(false);
        panelQuality.gameObject.SetActive(false);
        panelAuido.gameObject.SetActive(false);
        panelControlls.gameObject.SetActive(false);
    }
    //打开目标panel
    private void OpenPanel(GameObject targetPanel)
    {
        CloseAllPanels();
        targetPanel.SetActive(true);
        AudioManager.Instance.Play("泡泡音");
    }
  
   





   
}