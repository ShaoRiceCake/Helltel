using Obi;
using UnityEngine;

[RequireComponent(typeof(PlayerControlInformationProcess))]
public abstract class PlayerControl_BaseControl : MonoBehaviour
{
    protected PlayerControlInformationProcess controlHandler;
    public ObiParticleAttachment particleAttachment;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        controlHandler = GetComponent<PlayerControlInformationProcess>();
    }

}
