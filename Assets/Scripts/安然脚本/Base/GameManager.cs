using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class GameManager : Singleton<GameManager>
{
    public UnityEvent OnStartGame;

    public bool isGameing;

    private static bool _isDontDestroyOnLoad;

    public Dictionary<ulong, PlayerInfo> AllPlayerInfos { get; private set; }
    public Dictionary<ulong, NetworkSpawn> AllPlayers;

    public List<Transform> playerIdentifiers;

    public string joinConde;

    protected override void Awake()
    {
        base.Awake();
        AllPlayerInfos = new Dictionary<ulong, PlayerInfo>();
        AllPlayers = new Dictionary<ulong, NetworkSpawn>();
        playerIdentifiers = new List<Transform>();
        if (_isDontDestroyOnLoad && GameObject.FindGameObjectsWithTag("NetworkManager").Length > 1)
        {
            Destroy(GameObject.FindGameObjectsWithTag("NetworkManager")[1].gameObject);
        }

        if (!_isDontDestroyOnLoad)
        {
            _isDontDestroyOnLoad = true;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // 事件总线
        EventBus<PlayerInitializeEvent>.Subscribe(RegisterPlayer, this);
        // EventBus<PlayerInitializeEvent>.Subscribe(UnregistPlayer, this);
    }

    
    private void RegisterPlayer(PlayerInitializeEvent evt)
    {
        Debug.Log("监听到注册玩家标识符事件" + evt.Identifier.name);
        if (evt.Identifier != null && !playerIdentifiers.Contains(evt.Identifier))
        {
            Debug.Log("注册玩家标识符" + evt.Identifier.name);
            playerIdentifiers.Add(evt.Identifier);
        }
        else
        {
            Debug.LogError("玩家标识符为空或已存在");
        }
    }
    private void UnregistPlayer(PlayerInitializeEvent evt)
    {
        if (evt.Identifier != null && playerIdentifiers.Contains(evt.Identifier))
        {
            playerIdentifiers.Remove(evt.Identifier);
        }
        else
        {
            Debug.LogError("玩家标识符为空或不存在");
        }
    }
    private void ODestroy()
    {
        // 取消事件总线订阅
        EventBus<PlayerInitializeEvent>.UnsubscribeAll(this);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventComplete;

    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventComplete;

        base.OnNetworkDespawn();
    }

    private void OnLoadEventComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == "联机电梯")
        {
            OnStartGame?.Invoke();
            isGameing = true;
            Debug.Log("游戏开始");
        }

    }

    public void LoadScene(string sceneName)
    {
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void LoadSceneAddtive(string sceneName)
    {
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public void UnLoadScene(string sceneName)
    {
        NetworkManager.SceneManager.UnloadScene(SceneManager.GetSceneByName(sceneName));
    }

    public void StartGame(Dictionary<ulong, PlayerInfo> playerInfo)
    {
        AllPlayerInfos = playerInfo;

        UpdateAllPlayerInfosServerRpc();
    }

    public void Reset()
    {
        AllPlayerInfos.Clear();
        OnStartGame.RemoveAllListeners();
        isGameing = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateAllPlayerInfosServerRpc()
    {
        foreach (var playerInfo in AllPlayerInfos)
        {
            UpdatePlayerInfoClientRpc(playerInfo.Value);
        }
    }

    [ClientRpc]
    void UpdatePlayerInfoClientRpc(PlayerInfo playerInfo)
    {
        if (!IsServer)
        {
            if (AllPlayerInfos.ContainsKey(playerInfo.id))
            {
                AllPlayerInfos[playerInfo.id] = playerInfo;
            }
            else
            {
                AllPlayerInfos.Add(playerInfo.id, playerInfo);
            }

        }

    }


    [Rpc(SendTo.Everyone)]
    public void HostExitRpc()
    {
        Reset();
        SceneManager.LoadScene("Main");
    }

    [Rpc(SendTo.Server)]
    public void ClientExitRpc(ulong id)
    {
        NetworkManager.Singleton.DisconnectClient(id);

    }

    public void SetJoinCode(string code)
    {
        joinConde = code;
    }
}

[Serializable]
public struct PlayerInfo : INetworkSerializable
{
    public ulong id;

    public bool isReady;

    public string name;

    public PlayerInfo(ulong id, string name, bool isReady)
    {
        this.id = id;
        this.name = name;
        this.isReady = isReady;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref isReady);
    }
}
