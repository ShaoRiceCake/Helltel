using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class MoneyBag : NetworkBehaviour
{
    public NetworkVariable<float> money = new NetworkVariable<float>();
    [Rpc(SendTo.Server)]
    public void SetMoneyServerRpc(float m)
    {
        money.Value = m;
    }
}
