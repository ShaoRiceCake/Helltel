using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour {
    public static SettingsManager Instance; // 单例

    [SerializeField] private GameObject settingsPanel; // 预制体引用
    private GameObject currentSettingsUI;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    // 打开设置界面
    public void OpenSettings() {
        if (currentSettingsUI == null) {
            currentSettingsUI = Instantiate(settingsPanel);
            currentSettingsUI.transform.SetParent(transform); // 挂载到管理器
        }
        currentSettingsUI.SetActive(true);
    }

    // 关闭设置界面
    public void CloseSettings() {
        if (currentSettingsUI != null) {
            currentSettingsUI.SetActive(false);
            // 或 Destroy(currentSettingsUI); 根据需求选择
        }
    }
}