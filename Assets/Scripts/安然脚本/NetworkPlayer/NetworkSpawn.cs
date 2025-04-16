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
public class NetworkSpawn : NetworkBehaviour
{
    
    public NetworkVariable<float> _syncHealth = new NetworkVariable<float>();//生命
    public NetworkVariable<float> _syncMoney = new NetworkVariable<float>();//钱数
    public NetworkVariable<float> _syncPerformence = new NetworkVariable<float>();//绩效
    public TMP_Text health_text;

    public PlayerInfo my_Info;
    [Header("联机镜像隐藏物体")]
    public GameObject[] thirdDestory;
    public GameObject NetworkMap;
    public GameObject colliderWorld;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            TakeDamageRpc(100);
        }


        if (!IsLocalPlayer)
        {
            for(int i = 0; i < thirdDestory.Length; i++)
            {
                thirdDestory[i].SetActive(false);
            }
            Destroy(colliderWorld);
        }

        GameManager.instance.OnStartGame.AddListener(() =>
        {
            my_Info = GameManager.instance.AllPlayerInfos[OwnerClientId];
            transform.position = GameObject.Find("Spawn1").transform.position;
        });
        base.OnNetworkSpawn();

    }
    [Rpc(SendTo.Server)]
    public void TakeDamageRpc(float damage)
    {
        _syncHealth.Value += damage;
        _syncHealth.Value = Mathf.Clamp(_syncHealth.Value, 0, 100f);
        UpdateHealthViewRpc();
    }
    [Rpc(SendTo.Server)]
    public void TakeMoneyRpc(float money)
    {
        _syncHealth.Value += money;
        _syncHealth.Value = Mathf.Clamp(_syncHealth.Value, 0, 100f);
    }
    [Rpc(SendTo.Server)]
    public void TakePerformenceRpc(float performence)
    {
        _syncHealth.Value += performence;
        _syncHealth.Value = Mathf.Clamp(_syncHealth.Value, 0, 100f);
    }
    [Rpc(SendTo.Owner)]
    void UpdateHealthViewRpc()
    {
        health_text.text = $"HP:{_syncHealth.Value}";
    }
}
