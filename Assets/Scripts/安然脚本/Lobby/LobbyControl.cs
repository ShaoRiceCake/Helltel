using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using System;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine.SceneManagement;
using Michsky.LSS;

public class LobbyControl : NetworkBehaviour
{
    [Header("玩家列表")]
    public Transform _content;
    [Header("房间中的玩家Cell")]
    public GameObject _originCell;
    [Header("开始游戏（仅主机）")]
    public Button _startBtn;
    [Header("退出游戏")]
    public Button _exitBtn;
    [Header("准备Toggle")]
    public Toggle _ready;
    [Header("是否准备")]
    public TMP_Text _readyornot;
    [Header("房间代码")]
    public TMP_Text roomCode;
    [Header("自定义名称")]
    public TMP_InputField _name;

    public bool exiting;

    Dictionary<ulong, PlayerListCell> _cellDictionary;
    Dictionary<ulong, PlayerInfo> _allPlayerInfo;
    public LoadingScreenManager lSS_Manager;


    public override void OnNetworkSpawn()
    {

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConn;

        }
        lSS_Manager = FindObjectOfType<LoadingScreenManager>();
        _allPlayerInfo = new Dictionary<ulong, PlayerInfo>();
        _startBtn.onClick.AddListener(OnStartClick);
        _exitBtn.onClick.AddListener(OnExitClick);
        _ready.onValueChanged.AddListener(OnReadyToggle);
        _cellDictionary = new Dictionary<ulong, PlayerListCell>();
        _name.onEndEdit.AddListener(OnEndEdit);
        roomCode.text = "房间代码:"+GameManager.instance.joinConde;
        PlayerInfo playerInfo = new PlayerInfo();
        playerInfo.id = NetworkManager.LocalClientId;
        playerInfo.name = "玩家"+playerInfo.id;
        _name.text = playerInfo.name;
        playerInfo.isReady = false;

        AddPlayer(playerInfo);

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConn;

        }

        base.OnNetworkDespawn();
    }

    private void OnEndEdit(string arg0)
    {
        if (string.IsNullOrEmpty(arg0))
        {
            return;
        }
        PlayerInfo playerInfo = _allPlayerInfo[NetworkManager.LocalClientId];
        playerInfo.name = arg0;
        _allPlayerInfo[NetworkManager.LocalClientId] = playerInfo;
        _cellDictionary[NetworkManager.LocalClientId].UpdateInfo(playerInfo);
        if (IsServer)
        {
            UpdateAllPlayerInfos();
        }
        else
        {
            UpdateAllPlayerInfosServerRpc(playerInfo);
        }

    }

    private void OnClientConn(ulong obj)
    {
        PlayerInfo playerInfo = new PlayerInfo();
        playerInfo.id =obj ;
        playerInfo.name = "玩家" + obj;
        playerInfo.isReady = false;
        AddPlayer(playerInfo);
        UpdateAllPlayerInfos();
        GameManager.instance.StartGame(_allPlayerInfo);
    }

    public void RemovePlayer(ulong id)
    {
        Destroy(_cellDictionary[id].gameObject);
        _allPlayerInfo.Remove(id);
        _cellDictionary.Remove(id); 
    }

    public void AddPlayer(PlayerInfo playerInfo)
    {
        _allPlayerInfo.Add(playerInfo.id, playerInfo);
        GameObject clone = Instantiate(_originCell);
        clone.transform.SetParent(_content, false);
        PlayerListCell cell = clone.GetComponent<PlayerListCell>();
        _cellDictionary.Add(playerInfo.id, cell);
        cell.Initial(playerInfo);
        clone.SetActive(true);
    }

    void UpdateAllPlayerInfos()
    {
        bool Cango = true;

        foreach(var item in _allPlayerInfo)
        {
            if (!item.Value.isReady)
            {
                Cango = false;
            }

            UpdatePlayerInfoClientRpc(item.Value);
        }
        
        _startBtn.gameObject.SetActive(Cango);
    }

    private void UpdatePlayerCells()
    {
        foreach(var item in _allPlayerInfo)
        {
            _cellDictionary[item.Key].UpdateInfo(item.Value);
        }
    }

    private void OnReadyToggle(bool arg0)
    {
        _readyornot.text= arg0 ? "已准备" : "未准备";
        _cellDictionary[NetworkManager.LocalClientId].SetReady(arg0);
        UpdatePlayerInfo(NetworkManager.LocalClientId, arg0);
        if (IsServer)
        {
            UpdateAllPlayerInfos();
        }
        else
        {
            UpdateAllPlayerInfosServerRpc(_allPlayerInfo[NetworkManager.LocalClientId]);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    void UpdateAllPlayerInfosServerRpc(PlayerInfo player)
    {
        _allPlayerInfo[player.id] = player;
        _cellDictionary[player.id].UpdateInfo(player);
        UpdateAllPlayerInfos();
    }

    [ClientRpc]
    void UpdatePlayerInfoClientRpc(PlayerInfo playerInfo)
    {
        if (!IsServer)
        {
            if (_allPlayerInfo.ContainsKey(playerInfo.id))
            {
                _allPlayerInfo[playerInfo.id] = playerInfo;
            }
            else
            {
                AddPlayer(playerInfo);
            }
            UpdatePlayerCells();
        }

    }

    void UpdatePlayerInfo(ulong id,bool isReady)
    {
        PlayerInfo info = _allPlayerInfo[id];
        info.isReady = isReady;
        _allPlayerInfo[id] = info;
    }

    private void OnStartClick()
    {
        GameManager.instance.StartGame(_allPlayerInfo);

        GameManager.instance.LoadScene("联机电梯");
    }

    private void OnExitClick()
    {
        if (IsHost)
        {
            GameManager.instance.HostExitRpc();
            foreach (NetworkClient g in NetworkManager.Singleton.ConnectedClients.Values)
            {
                g.PlayerObject.GetComponent<NetworkObject>().Despawn();
            }

            NetworkManager.Singleton.Shutdown();
        }
        else
        {
            exiting = true;
            GameManager.instance.Reset();
            DespawnServerRpc(NetworkManager.Singleton.LocalClientId);
            UpdateRoomCellRpc(NetworkManager.Singleton.LocalClientId);
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("Main");
        }
    }

    [Rpc(SendTo.NotMe)]
    private void DespawnServerRpc(ulong localClientId)
    {
        if (IsServer)
        {
            NetworkManager.Singleton.ConnectedClients[localClientId].PlayerObject.GetComponent<NetworkObject>().Despawn();
        }

    }

    [Rpc(SendTo.NotMe)]
    void UpdateRoomCellRpc(ulong id)
    {
        Destroy(_cellDictionary[id].gameObject);
        _cellDictionary.Remove(id);
        _allPlayerInfo.Remove(id);

    }

}
