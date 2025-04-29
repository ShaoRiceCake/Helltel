using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Sync.Relay.Lobby;
using Unity.Sync.Relay.Model;
using Unity.Sync.Relay.Transport.Netcode;
using UnityEngine.SceneManagement;
using Michsky.LSS;

public class StandaloneMainMenu : MonoBehaviour
{
    [Header("UI 元素")]
    
    public Button btn_SinglePlayer;                     // 单机游戏按钮
    public Button btn_OpenOnlineMultiplayer;                   // 加入客户端按钮

    public GameObject onlineMultiplayerPanel;           //加入房间面板

    public Button btn_ConfirmJoinRoom;     //加入房间的确认按钮
    public Button btn_ExitJoinRoom;        //加入房间的退出按钮
    public Button btn_SettingOfMainMenu;        //主菜单的设置按钮
    public Button btn_GuestBookOfMainMenu;      //主菜单里的客人图鉴按钮



    [Header("其他组件")]
    public GlobalUIController globalUIController; //全局UI脚本
    public LoadingScreenManager lSS_Manager;


    private void Start()
    {
       

        onlineMultiplayerPanel.SetActive(false);
        
 
        // 绑定按钮事件
        btn_SinglePlayer.onClick.AddListener(SinglePlayerButton);
        btn_OpenOnlineMultiplayer.onClick.AddListener(OpenOnlineMultiplayerPanel);
        btn_ConfirmJoinRoom.onClick.AddListener(CloseOnlineMultiplayerPanel);
     

        btn_SettingOfMainMenu.onClick.AddListener(globalUIController.OpenSettings);
        btn_GuestBookOfMainMenu.onClick.AddListener(globalUIController.OpenGuestBook);
    }


    /// <summary>
    /// 创建单人游戏按钮点击事件
    /// </summary>
    public void SinglePlayerButton()
    {
        lSS_Manager.LoadScene("单机正式电梯");
    }






    /// <summary>
    /// 打开多人联机界面
    /// </summary>
    public void OpenOnlineMultiplayerPanel()
    {
        onlineMultiplayerPanel.SetActive(true);
    }
    /// <summary>
    /// 关闭加入房间界面
    /// </summary>
    public void CloseOnlineMultiplayerPanel()
    {
        onlineMultiplayerPanel.SetActive(false);
    }

    private void Update() { }



}