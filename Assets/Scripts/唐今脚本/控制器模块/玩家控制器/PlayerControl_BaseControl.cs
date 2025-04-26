using UnityEngine;
using Unity.Netcode;
[RequireComponent(typeof(PlayerControlInformationProcess))]
public class PlayerControl_BaseControl : NetworkBehaviour
{
    public PlayerControlInformationProcess controlHandler;
    public GameObject forwardObject;
    protected ulong ClientID;

    protected virtual void Start()
    {
        controlHandler = GetComponent<PlayerControlInformationProcess>();

        NullCheckerTool.CheckNull(forwardObject, controlHandler);
        
        ClientID = NetworkManager.ServerClientId;
    }
}
