using UnityEngine;
using Unity.Netcode;
[RequireComponent(typeof(PlayerControlInformationProcess))]
public class PlayerControl_BaseControl : NetworkBehaviour
{
    public PlayerControlInformationProcess controlHandler;
    public GameObject forwardObject;
    

    virtual protected void Start()
    {
        controlHandler = GetComponent<PlayerControlInformationProcess>();

        NullCheckerTool.CheckNull(forwardObject, controlHandler);
    }
}
