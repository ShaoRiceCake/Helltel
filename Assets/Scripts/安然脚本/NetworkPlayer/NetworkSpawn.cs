using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class NetworkSpawn : NetworkBehaviour
{
    [Header("联机镜像隐藏物体")]
    public GameObject[] thirdDestory;
    public GameObject NetworkMap;
    public GameObject colliderWorld;
    public override void OnNetworkSpawn()
    {
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
            if (IsLocalPlayer)
            {
                GameController.Instance.NotifyLocalPlayerReady(NetworkManager.Singleton.LocalClientId.ToString());
            }

            transform.position = GameObject.Find("Spawn1").transform.position;
        });
        base.OnNetworkSpawn();

    }

}
