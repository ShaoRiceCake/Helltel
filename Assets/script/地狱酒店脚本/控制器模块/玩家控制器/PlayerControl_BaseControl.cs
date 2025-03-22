using UnityEngine;

[RequireComponent(typeof(PlayerControlInformationProcess))]
public abstract class PlayerControl_BaseControl : MonoBehaviour
{
    protected PlayerControlInformationProcess controlHandler;
    public GameObject forwardObject;

    virtual protected void Start()
    {
        controlHandler = GetComponent<PlayerControlInformationProcess>();

        NullCheckerTool.CheckNull(forwardObject, controlHandler);
    }

}
