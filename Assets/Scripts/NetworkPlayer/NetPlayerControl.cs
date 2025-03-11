using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerControl : NetworkBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.isGameing) return;

        if (IsLocalPlayer)
        {
            
        }
    }
}
