using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    private GlobalUIController globalUIController;
    
    [Header("菜单界面按钮")]
    public Button btnContinue;  // 继续游戏
    public Button btnSettings;  // 设置按钮
    public Button btnGuestBook; //宾客簿
    public Button btnKillMe;    //自杀
    public Button btnBackToMainMenu;    //返回主菜单
    public Button btnQuit;      // 退出游戏

    
    private void Awake()
    {
        
    }

    private void Start()
    {
        globalUIController = GlobalUIController.Instance.GetComponent<GlobalUIController>();
        // 绑定按钮事件
        btnContinue.onClick.AddListener(Continue);
        btnSettings.onClick.AddListener(globalUIController.OpenSettings);
        btnGuestBook.onClick.AddListener(globalUIController.OpenGuestBook);
        btnKillMe.onClick.AddListener(KillMe);
        btnBackToMainMenu.onClick.AddListener(BackToMainMenu);
        btnQuit.onClick.AddListener(QuitGame);
        
        
    }

    private void Update()
    {
       

    }
    //继续
    private void Continue()
    {
        globalUIController.SetPause(false);
        globalUIController.CloseAllGlobalUI();
        AudioManager.Instance.Play("泡泡音");
    }



    // //打开宾客簿
    // private void OpenGuestBook()
    // {
    //     globalUIController.CloseAllGlobalUI();
    //     globalUIController.GuestBook.gameObject.SetActive(true);
    // }
    //自杀
    private void KillMe()
    {
        
        //这里预留自杀逻辑
    }
    //返回主菜单
    public static void BackToMainMenu()
    {
        SceneManager.LoadSceneAsync("开始场景");
    }
    
    //关闭游戏
    private void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}