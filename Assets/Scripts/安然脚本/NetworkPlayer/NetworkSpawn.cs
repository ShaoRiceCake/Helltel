using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class NetworkSpawn : NetworkBehaviour
{
    public GameObject playerCam;
    public GameObject virtualCam;
    public GameObject NetworkMap;
    public GameObject colliderWorld;
    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer)
        {
            Destroy(playerCam);
            Destroy(virtualCam);
            Destroy(colliderWorld);
        }

        GameManager.instance.OnStartGame.AddListener(() =>
        {



            if (IsHost && IsLocalPlayer)
            {

                var obj = Instantiate(NetworkMap);
                StartCoroutine(obj.GetComponent<DungeonGenerator>().DungeonBuild());

            }

            transform.position = GameObject.Find("Spawn1").transform.position;
        });
        base.OnNetworkSpawn();

    }

}
