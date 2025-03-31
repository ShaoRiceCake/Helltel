using UnityEngine;

[RequireComponent(typeof(PlayerControlInformationProcess))]
public abstract class PlayerControl_BaseControl : MonoBehaviour
{
    protected PlayerControlInformationProcess ControlHandler;
    public GameObject forwardObject;

    protected virtual void Start()
    {
        ControlHandler = GetComponent<PlayerControlInformationProcess>();

        NullCheckerTool.CheckNull(forwardObject, ControlHandler);
    }

}
