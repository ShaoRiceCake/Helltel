using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GlobalUIController : MonoBehaviour
{
    // 传统单例模式
    public static GlobalUIController Instance { get; private set; }

    public MenuPanel Menu;
    public SettingsPanel Settings;
    public GuestBookPanel GuestBook;
    public GameManager gameManager;
    private GameDataModel _data;
    private  bool canPressESC = true;

    // // 属性访问（无需基类）
    // public MenuPanel Menu => _menu;
    // public SettingsPanel Settings => _settings;
    // public GuestBookPanel GuestBook => _guestBook;

    //public bool isPaused = false;

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
    private void OnEnable()
    {
        _data = Resources.Load<GameDataModel>("GameData");
        _data.StartLoading += HandleStartLoading;
        _data.FinishLoading += HandleFinishLoading;
        
    }
    private void OnDisable()
    {
        _data.StartLoading -= HandleStartLoading;
        _data.FinishLoading -= HandleFinishLoading;
    }
   
    private void Update()
    {
        HandleEscapeInput();
    }

    private void HandleEscapeInput()
    {
        // if (!Input.GetKeyDown(KeyCode.Escape)) return;


        // //根据当前界面状态处理
        // //当处于设置或宾客簿界面时
        // if (Settings.gameObject.activeSelf || GuestBook.gameObject.activeSelf)
        // {
        //     if(gameManager.isGameing == true )
        //     {
        //         // 从二级界面返回菜单
        //         OpenMenu();
        //     }
        //     else
        //     {
        //         CloseAllGlobalUI();
        //         AudioManager.Instance.Play("泡泡音");
        //     }
            
        // }
        // //当处于菜单界面时
        // else if (Menu.gameObject.activeSelf)
        // {
        //     // 关闭菜单返回游戏,继续游戏
        //     ReturnGame();
            
        // }
        // //当处于游戏场景且未打开UI界面
        // else
        // {
        //     if(gameManager.isGameing == true )
        //     {
        //         // 打开菜单
        //         OpenMenu();
                
        //         Time.timeScale = 0;
        //     }
        // }
        if(canPressESC == false)return;
        if (!Input.GetKeyDown(KeyCode.Escape)) return;


        //根据当前界面状态处理
        //当处于设置或宾客簿界面时
        if (Settings.gameObject.activeSelf || GuestBook.gameObject.activeSelf)
        {
            if(SceneManager.GetActiveScene().name != "单机正式主菜单")
            {
                // 从二级界面返回菜单
                OpenMenu();
            }
            else
            {
                CloseAllGlobalUI();
                AudioManager.Instance.Play("泡泡音");
            }
            
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
            if(SceneManager.GetActiveScene().name != "单机正式主菜单")
            {
                // 打开菜单
                OpenMenu();
                
                Time.timeScale = 0;
            }
        }
    }
   

    // 设置暂停/继续游戏
    public void SetPause(bool isPaused)
    {
        Time.timeScale = isPaused ? 0 : 1;
    
        StartCoroutine(SetCursorStateWithDelay(isPaused));
    }

    private static IEnumerator SetCursorStateWithDelay(bool isPaused)
    {
        yield return null;
    
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    
        var playersControlInformation = FindObjectOfType<PlayerControlInformationProcess>();
        if (playersControlInformation)
        {
            playersControlInformation.stopPlayerControl = isPaused;
        }
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
        if(SceneManager.GetActiveScene().name != "单机正式主菜单")
        {
            SetPause(false);
        }
        AudioManager.Instance.Play("泡泡音");
    }
    //打开菜单
    public void OpenMenu()
    {
        CloseAllGlobalUI();
        if(SceneManager.GetActiveScene().name != "单机正式主菜单")
        {
            SetPause(true);
        }
        Menu.gameObject.SetActive(true);
        AudioManager.Instance.Play("泡泡音");
    }
    //打开设置
    public void OpenSettings()
    {
        CloseAllGlobalUI();
        Settings.gameObject.SetActive(true);
        AudioManager.Instance.Play("泡泡音");
    }
    //关闭设置
    public void CloseSettings()
    {
        if(SceneManager.GetActiveScene().name != "单机正式主菜单")
        {
            Debug.Log("不是主菜单");
            OpenMenu();
            AudioManager.Instance.Play("泡泡音");
        }
        else if(SceneManager.GetActiveScene().name == "单机正式主菜单")
        {
            bool isPaused = true;
            Cursor.lockState = isPaused ? CursorLockMode.None: CursorLockMode.Locked ; // 控制鼠标锁定状态
            Cursor.visible = isPaused; // 控制鼠标可见性
            CloseAllGlobalUI();
            AudioManager.Instance.Play("泡泡音");
        }
      
    }
    //打开宾客簿
    public void OpenGuestBook()
    {
        CloseAllGlobalUI();
        GuestBook.gameObject.SetActive(true);
        AudioManager.Instance.Play("泡泡音");
    }
    //关闭宾客簿
    public void CloseGuestBook()
    {
        if(SceneManager.GetActiveScene().name != "单机正式主菜单")
        {
            OpenMenu();
        }
        else if(SceneManager.GetActiveScene().name == "单机正式主菜单")
        {
            bool isPaused = true;
            Cursor.lockState = isPaused ? CursorLockMode.None: CursorLockMode.Locked ; // 控制鼠标锁定状态
            Cursor.visible = isPaused; // 控制鼠标可见性
            CloseAllGlobalUI();
            AudioManager.Instance.Play("泡泡音");
        }
    }
    public void HandleStartLoading()
    {
        SetPause(true);
        canPressESC = false;
    }
    public void HandleFinishLoading()
    {
        SetPause(false);
        canPressESC = true;
    }
    
}