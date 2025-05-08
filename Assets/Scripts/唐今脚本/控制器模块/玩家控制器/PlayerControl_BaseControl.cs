using UnityEngine;
using Unity.Netcode;
[RequireComponent(typeof(PlayerControlInformationProcess))]
public class PlayerControl_BaseControl : MonoBehaviour
{
    public PlayerControlInformationProcess controlHandler;
    public GameObject forwardObject;

    public ParticleSystem controlParticleEffect; // 拖拽粒子特效预制体到这个变量
    protected bool shouldShowParticles = false;

    protected virtual void Start()
    {
        controlHandler = GetComponent<PlayerControlInformationProcess>();

        NullCheckerTool.CheckNull(forwardObject, controlHandler);
        
    }
}
