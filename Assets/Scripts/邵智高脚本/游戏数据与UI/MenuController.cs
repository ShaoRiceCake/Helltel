using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance { get; private set; }
    
    [Header("主菜单")]
    public GameObject mainMenu; // 主菜单面板
    public Button btnSettings; // 设置按钮
    
    [Header("其他按钮")]
    public Button btnContinue; // 继续游戏
    public Button btnKill;//自杀
    public Button btnQuit;    // 退出游戏

    private bool isPaused;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 绑定按钮事件
        btnContinue.onClick.AddListener(TogglePause);
        btnSettings.onClick.AddListener(OpenSettings);
        btnQuit.onClick.AddListener(QuitGame);
        
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // 暂停/继续游戏
    public void TogglePause()
    {
        isPaused =  !isPaused;
        Time.timeScale = isPaused ? 0 : 1; // 控制游戏时间流速（0暂停/1正常）
        mainMenu.SetActive(isPaused);
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked; // 控制鼠标锁定状态
        Cursor.visible = isPaused; // 控制鼠标可见性
    }

    // 打开设置界面
    private void OpenSettings()
    {
        SettingsController.Instance.ShowSettings();
        mainMenu.SetActive(false);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}