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

public class NetworkRoom : MonoBehaviour
{

    public TMP_InputField inputIP;

    public Button host;

    public Button client;

    public GameObject loadingPanel;

    public Button back_btn;

    public TMP_Text title;

    public RelayTransportNetcode relayTransportNetcode;

    void Loading(string str)
    {
        title.text = str;
        back_btn.gameObject.SetActive(true);
    }

    string uid;
    private void Start()
    {
        relayTransportNetcode = NetworkManager.Singleton.GetComponentInChildren<RelayTransportNetcode>();
        loadingPanel.SetActive(false);
        uid = Guid.NewGuid().ToString();
        var props = new Dictionary<string, string>();
        props.Add("icon", "unity");
        relayTransportNetcode.SetPlayerData(uid, "Player-" + uid, props);
        host.onClick.AddListener(OnStartHostButton);
        client.onClick.AddListener(OnStartClientButton);
        back_btn.onClick.AddListener(() =>
        {
            loadingPanel.SetActive(false);
            back_btn.gameObject.SetActive(false);
        });
    }

    public void OnStartHostButton()
    {
        loadingPanel.SetActive(true);
        title.text = "创建房间中...";
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransportNetcode;

        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening)
        {
            if (true)
            {
                StartCoroutine(LobbyService.AsyncCreateRoom(new CreateRoomRequest()
                {
                    Name = "Demo",
                    Namespace = "Unity",
                    MaxPlayers = 20,
                    Visibility = LobbyRoomVisibility.Public,
                    OwnerId = uid,
                    CustomProperties = new Dictionary<string, string>()
                    {
                        {"a", "b"},
                    }
                }, (resp) =>
                {
                    if (resp.Code == (uint)RelayCode.OK)
                    {
                        Debug.Log("Create Room succeed.");
                        if (resp.Status == LobbyRoomStatus.ServerAllocated)
                        {
                            relayTransportNetcode.SetRoomData(resp);
                            StartHost();
                            GameManager.instance.joinConde = resp.RoomCode;
                            GameManager.instance.LoadScene("Lobby");
                        }
                        else
                        {
                            Loading("创建房间失败，当前状态：" + resp.Status.ToString());
                            Debug.Log("Room Status Exception : " + resp.Status.ToString());
                        }
                    }
                    else
                    {
                        Loading("创建房间失败：未进入服务器");
                        Debug.Log("Create Room Fail By Lobby Service");
                    }
                }));


            }
        }
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void OnStartClientButton()//以 client 身份加入游戏
    {
        loadingPanel.SetActive(true);
        title.text = "加入房间中...";
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransportNetcode;

        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening)
        {
            if (true)
            {

                StartCoroutine(LobbyService.AsyncListRoom(new ListRoomRequest()
                {
                    Namespace = "Unity",
                    Start = 0,
                    Count = 10,
                }, (resp) =>
                {
                    if (resp.Code == (uint)RelayCode.OK)
                    {
                        Debug.Log("List Room succeed.");
                        if (resp.Items.Count > 0)
                        {
                            foreach (var item in resp.Items)
                            {
                                if (item.Status == LobbyRoomStatus.Ready)
                                {
                                    StartCoroutine(LobbyService.AsyncQueryRoom(item.RoomUuid,
                                        (_resp) =>
                                        {
                                            Debug.Log(item.RoomCode);
                                            if (_resp.Code == (uint)RelayCode.OK && item.RoomCode == inputIP.text)
                                            {
                                                Debug.Log("Query Room succeed.");
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
                                                    Loading("房间失效");
                                                }
                                       

                                                Debug.Log("Query Room Fail By Lobby Service");
                                            }
                                        }));
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Loading("当前服务器无房间");
                        }
                    }
                    else
                    {
                        Loading("房间失效");

                        Debug.Log("List Room Fail By Lobby Service");
                    }
                }));
            }
        }
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    #region 本地测试
    public void Login()
    {
        NetworkManager.Singleton.StartClient();
        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData("127.0.0.1" ,7777);

    }//登录

    public void BeHost()
    {

        UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData("0.0.0.0", 7777);
        NetworkManager.Singleton.StartHost();
        GameManager.instance.LoadScene("Lobby");
    }//主机启动
    #endregion
}
