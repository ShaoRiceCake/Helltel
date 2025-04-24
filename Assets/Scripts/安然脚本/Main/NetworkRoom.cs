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
/// <summary>
/// 网络房间管理器
/// 功能：
/// 1. 使用Relay服务创建/加入房间
/// 2. 管理网络连接流程
/// 3. 处理UI交互与状态显示
/// 4. 本地开发测试支持
/// </summary>
public class NetworkRoom : MonoBehaviour
{
    [Header("UI 元素")]
    public TMP_InputField inputIP;          // 房间代码输入框
    public Button btn_Host;                     // 创建主机按钮
    public Button btn_Client;                   // 加入客户端按钮
    public GameObject loadingPanel;        // 加载提示面板
    public GameObject joinPanel;           //加入房间面板
    public Button btn_Back;                 // 返回按钮
    public Button btn_ConfirmJoinRoom;     //加入房间的确认按钮
    public Button btn_ExitJoinRoom;        //加入房间的退出按钮
    public Button btn_SettingOfMainMenu;        //主菜单的设置按钮
    public Button btn_GuestBookOfMainMenu;      //主菜单里的客人图鉴按钮
    public TMP_Text title;                  // 状态提示文本

    [Header("网络组件")]
    public RelayTransportNetcode relayTransportNetcode; // Relay网络传输组件
    [Header("其他组件")]
    public GlobalUIController globalUIController; //全局UI脚本
    public LoadingScreenManager lSS_Manager;
    private string uid; // 玩家唯一标识

    private void Start()
    {
        // 初始化网络组件
        relayTransportNetcode = NetworkManager.Singleton.GetComponentInChildren<RelayTransportNetcode>();
        loadingPanel.SetActive(false);
        joinPanel.SetActive(false);
        
        // 生成玩家唯一ID
        uid = Guid.NewGuid().ToString();
        
        // 设置玩家数据
        var props = new Dictionary<string, string>();
        props.Add("icon", "unity");
        relayTransportNetcode.SetPlayerData(uid, "Player-" + uid, props);

        // 绑定按钮事件
        btn_Host.onClick.AddListener(BeHost);
        btn_Client.onClick.AddListener(OpenJoinPanel);
        btn_ConfirmJoinRoom.onClick.AddListener(Login);
        btn_ExitJoinRoom.onClick.AddListener(CloseJoinPanel);
        btn_Back.onClick.AddListener(() =>
        {
            loadingPanel.SetActive(false);
            btn_Back.gameObject.SetActive(false);
        });
        btn_SettingOfMainMenu.onClick.AddListener(globalUIController.OpenSettings);
        btn_GuestBookOfMainMenu.onClick.AddListener(globalUIController.OpenGuestBook);
    }

    /// <summary>
    /// 更新加载状态提示
    /// </summary>
    /// <param name="str">显示的状态文本</param>
    void Loading(string str)
    {
        title.text = str;
        btn_Back.gameObject.SetActive(true);
    }

    /// <summary>
    /// 创建主机按钮点击事件
    /// </summary>
    public void OnStartHostButton()
    {
        loadingPanel.SetActive(true);
        title.text = "创建房间中...";
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransportNetcode;

        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening)
        {
            // 发起创建房间请求
            StartCoroutine(LobbyService.AsyncCreateRoom(
                new CreateRoomRequest()
                {
                    Name = "Demo",
                    Namespace = "Unity",
                    MaxPlayers = 20,            // 最大玩家数量
                    Visibility = LobbyRoomVisibility.Public,
                    OwnerId = uid,
                    CustomProperties = new Dictionary<string, string>() { {"a", "b"} }
                }, 
                (resp) =>
                {
                    if (resp.Code == (uint)RelayCode.OK)
                    {
                        Debug.Log("房间创建成功");
                        if (resp.Status == LobbyRoomStatus.ServerAllocated)
                        {
                            // 配置网络参数
                            relayTransportNetcode.SetRoomData(resp);
                            StartHost();
                            GameManager.instance.joinConde = resp.RoomCode;
                            GameManager.instance.LoadScene("Lobby"); // 加载大厅场景
                        }
                        else
                        {
                            Loading($"创建房间失败，当前状态：{resp.Status}");
                            Debug.LogError($"房间状态异常：{resp.Status}");
                        }
                    }
                    else
                    {
                        Loading("创建房间失败：未连接服务器");
                        Debug.LogError("大厅服务创建房间失败");
                    }
                }));
        }
    }

    /// <summary>
    /// 启动主机
    /// </summary>
    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    /// <summary>
    /// 加入客户端按钮点击事件
    /// </summary>
    public void OnStartClientButton()
    {
        loadingPanel.SetActive(true);
        title.text = "加入房间中...";
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransportNetcode;

        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening)
        {
            // 获取房间列表
            StartCoroutine(LobbyService.AsyncListRoom(
                new ListRoomRequest()
                {
                    Namespace = "Unity",
                    Start = 0,   // 分页起始索引
                    Count = 10,  // 每页数量
                }, 
                (resp) =>
                {
                    if (resp.Code == (uint)RelayCode.OK)
                    {
                        Debug.Log("获取房间列表成功");
                        if (resp.Items.Count > 0)
                        {
                            foreach (var item in resp.Items)
                            {
                                if (item.Status == LobbyRoomStatus.Ready)
                                {
                                    // 查询具体房间信息
                                    StartCoroutine(LobbyService.AsyncQueryRoom(
                                        item.RoomUuid,
                                        (_resp) =>
                                        {
                                            Debug.Log($"尝试加入房间：{item.RoomCode}");
                                            if (_resp.Code == (uint)RelayCode.OK && item.RoomCode == inputIP.text)
                                            {
                                                Debug.Log("房间查询成功");
                                                relayTransportNetcode.SetRoomData(_resp);
                                                GameManager.instance.joinConde = item.RoomCode;
                                                StartClient();
                                            }
                                            else
                                            {
                                                if(item.RoomCode != inputIP.text)
                                                {
                                                    Loading("房间代码错误");
                                                }
                                                if(_resp.Code != (uint)RelayCode.OK)
                                                {
                                                    Loading("房间已失效");
                                                }
                                                Debug.LogError("大厅服务查询房间失败");
                                            }
                                        }));
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Loading("当前没有可用房间");
                        }
                    }
                    else
                    {
                        Loading("获取房间列表失败");
                        Debug.LogError("大厅服务列表请求失败");
                    }
                }));
        }
    }

    /// <summary>
    /// 启动客户端
    /// </summary>
    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    /// <summary>
    /// 打开加入房间界面
    /// </summary>
    public void OpenJoinPanel()
    {
        joinPanel.SetActive(true);
    }
    /// <summary>
    /// 关闭加入房间界面
    /// </summary>
    public void CloseJoinPanel()
    {
        joinPanel.SetActive(false);
    }

    private void Update() { }

    #region 本地测试方法
    /// <summary>
    /// 本地直接连接
    /// </summary>
    public void Login()
    {
        NetworkManager.Singleton.StartClient();
        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData("127.0.0.1", 7777); // 本地环回地址
    }

    /// <summary>
    /// 启动本地主机
    /// </summary>
    public void BeHost()
    {
        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData("0.0.0.0", 7777); // 监听所有地址
        NetworkManager.Singleton.StartHost();
        
        // // 设置目标场景名称
        // LoadingScreen.SceneLoader.TargetSceneName = "Lobby";
        // // 加载加载场景（单例模式）
        // SceneManager.LoadScene("Loading", LoadSceneMode.Single);   
        //SceneManager.LoadScene("Lobby");
        GameManager.instance.LoadScene("Lobby");
    }
    #endregion
}