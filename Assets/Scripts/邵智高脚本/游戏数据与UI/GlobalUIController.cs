using UnityEngine;

public class GlobalUIController : MonoBehaviour
{
    // 传统单例模式
    public static GlobalUIController Instance { get; private set; }

    public MenuPanel Menu;
    public SettingsPanel Settings;
    public GuestBookPanel GuestBook;

    // // 属性访问（无需基类）
    // public MenuPanel Menu => _menu;
    // public SettingsPanel Settings => _settings;
    // public GuestBookPanel GuestBook => _guestBook;

    public bool isPaused = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化隐藏
        Menu.gameObject.SetActive(false);
        Settings.gameObject.SetActive(false);
        GuestBook.gameObject.SetActive(false);
    }

    // 设置暂停/继续游戏
    public void TogglePause(bool isPaused)
    {
        Time.timeScale = isPaused ? 0 : 1; // 控制游戏时间流速（0暂停/1正常）
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked; // 控制鼠标锁定状态
        Cursor.visible = isPaused; // 控制鼠标可见性
    }
    public void CloseAllGlobalUI()
    {
        Menu.gameObject.SetActive(false);
        Settings.gameObject.SetActive(false);
        GuestBook.gameObject.SetActive(false);
    }
}