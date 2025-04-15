using UnityEngine;
using UnityEngine.SceneManagement;


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
   
    private void Update()
    {
        HandleEscapeInput();
    }

    private void HandleEscapeInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        
        if (!IsInGameScene())
        {
            // 非游戏场景不响应ESC
            return;
        }

        //根据当前界面状态处理
        //当处于设置或宾客簿界面时
        if (Settings.gameObject.activeSelf || GuestBook.gameObject.activeSelf)
        {
            // 从二级界面返回菜单
            OpenMenu();
        }
        //当处于菜单界面时
        else if (Menu.gameObject.activeSelf)
        {
            // 关闭菜单返回游戏,继续游戏
            ReturnGame();
            
        }
        //当处于游戏场景且未打开UI界面
        else
        {
            // 打开菜单
            OpenMenu();
        }
    }
    private bool IsInGameScene()
    {
        return true;
        // 请根据实际项目替换场景判断逻辑（示例：场景名为"Gameplay"）
        //return SceneManager.GetActiveScene().name == "Gameplay";
    }

    // 设置暂停/继续游戏
    public void SetPause(bool isPaused)
    {
        //Time.timeScale = isPaused ? 0 : 1; // 控制游戏时间流速（0暂停/1正常）由于我们是联机游戏，所以不暂停
        //这里其实根本没锁住，需要和角色操控联动
        Cursor.lockState = isPaused ? CursorLockMode.None: CursorLockMode.Locked ; // 控制鼠标锁定状态
        Cursor.visible = isPaused; // 控制鼠标可见性
    }
    //关闭所有全局UI界面
    public void CloseAllGlobalUI()
    {
        Menu.gameObject.SetActive(false);
        Settings.gameObject.SetActive(false);
        GuestBook.gameObject.SetActive(false);
    }
    //关闭菜单返回游戏
    public void ReturnGame()
    {
        CloseAllGlobalUI();
        SetPause(false);
        AudioManager.Instance.Play("泡泡音");
    }
    //打开菜单
    public void OpenMenu()
    {
        CloseAllGlobalUI();
        SetPause(true);
        Menu.gameObject.SetActive(true);
        AudioManager.Instance.Play("泡泡音");
    }
    //打开设置
    public void OpenSettings()
    {
        CloseAllGlobalUI();
        SetPause(true);
        Settings.gameObject.SetActive(true);
        AudioManager.Instance.Play("泡泡音");
    }
    //打开宾客簿
    public void OpenGuestBook()
    {
        CloseAllGlobalUI();
        SetPause(true);
        GuestBook.gameObject.SetActive(true);
        AudioManager.Instance.Play("泡泡音");
    }
    
}