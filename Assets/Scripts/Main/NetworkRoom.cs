using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
//using Unity.Services.Relay;
//using Unity.Services.Authentication;
//using Unity.Services.Core;
//using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using System;

public class NetworkRoom : MonoBehaviour
{

    public TMP_InputField inputIP;

    public Button host;

    public Button client;

    private void Start()
    {
        host.onClick.AddListener(BeHost); // 
        client.onClick.AddListener(Login); 
    }
    #region 广域网同步
    //private async void OnStartClientButtonClicked()
    //{

    //    GameManager.instance.SetJoinCode(inputIP.text);

    //    try
    //    {
    //        Isjoin = await StartClientWithRelay(inputIP.text);
    //    }
    //    catch (RelayServiceException ex)
    //    {
    //        if (ex.Message.StartsWith("Bad"))
    //        {
    //            Debug.Log("房间代码无效！");
    //        }
    //        // 处理 Relay 服务器错误（例如无效代码或连接失败）
    //        Debug.LogError("Relay Error: " + ex.Message);
    //    }
    //    catch (Exception ex)
    //    {
    //        if (ex.Message.StartsWith("Value"))
    //        {
    //            Debug.Log("房间代码不可为空");
    //        }
    //        // 处理其他异常
    //        Debug.LogError("General Error: " + ex.Message);
    //    }

    //}

    //private async void OnStartHostButtonClicked()
    //{
    //    GameManager.instance.SetJoinCode(await StartHostWithRelay(MaxConnectPlayers));
    //    GameManager.instance.LoadScene("Lobby");
    //}

    //public async Task<string> StartHostWithRelay(int maxConnections)
    //{
    //    await UnityServices.InitializeAsync();

    //    if (!AuthenticationService.Instance.IsSignedIn)
    //    {
    //        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    //    }

    //    Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
    //    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
    //    var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

    //    return NetworkManager.Singleton.StartHost() ? joinCode : null;
    //}


    //public async Task<bool> StartClientWithRelay(string joinCode)
    //{
    //    await UnityServices.InitializeAsync();
    //    if (!AuthenticationService.Instance.IsSignedIn)
    //    {
    //        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    //    }

    //    var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
    //    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
    //    return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    //}
    #endregion

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
