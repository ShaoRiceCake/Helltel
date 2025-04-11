using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{

    [Header("设置主界面")]
    public GameObject settingsPanel;
    
    [Header("标签页按钮")]
    public Button[] tabButtons;
    
    [Header("设置面板")]
    public GameObject[] settingPanels;

    [Header("音频设置")]
    public Slider masterVolumeSlider;

    [Header("图形设置")]
    public TMP_Dropdown qualityDropdown;

    private void Awake()
    {
        
    }

    private void InitializeSettings()
    {
        // 初始化标签页按钮
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => SwitchPanel(index));
        }
        
        // 初始化画质选项
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
    }

    // 显示设置界面
    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
        SwitchPanel(0); // 默认显示第一个面板
    }

    // 切换设置面板
    private void SwitchPanel(int index)
    {
        foreach (var panel in settingPanels)
        {
            panel.SetActive(false);
        }
        settingPanels[index].SetActive(true);
    }

    // 关闭设置界面
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        MenuPanel.Instance.TogglePause(); // 返回主菜单
    }

    // 应用音频设置
    public void ApplyAudioSettings()
    {
        AudioListener.volume = masterVolumeSlider.value;
    }

    // 应用图形设置
    public void ApplyGraphicsSettings()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
    }
}