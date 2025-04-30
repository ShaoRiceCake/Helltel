using UnityEngine;
using Unity.Netcode;
[RequireComponent(typeof(PlayerControlInformationProcess))]
public class PlayerControl_BaseControl : MonoBehaviour
{
    public PlayerControlInformationProcess controlHandler;
    public GameObject forwardObject;


    protected virtual void Start()
    {
        controlHandler = GetComponent<PlayerControlInformationProcess>();

        NullCheckerTool.CheckNull(forwardObject, controlHandler);
        
    }
}
