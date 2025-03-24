using UnityEngine;
using Obi;
using UnityEngine.Serialization;

public class CatchTool : MonoBehaviour
{
    [HideInInspector]
    public bool attachAimPos = false;
    public bool isCatching = true;
    public ObiParticleAttachment obiAttachment; 

    private SphereCollider _sphereCollider;
    private Transform _catchAimTrans;

    private GameObject CatchBall { get; set; }

    public GameObject IgnoredFatherObject { get; set; }

    private void Start()
    {
        CancelCatch();
        HandleAttachment();

        _sphereCollider = CatchBall.GetComponent<SphereCollider>();
        
        NullCheckerTool.CheckNull(CatchBall,_sphereCollider,IgnoredFatherObject,obiAttachment);
    }

    private void Update()
    {
        if (isCatching)
        {
            OnTriggerEnter(_sphereCollider);
        }
        else
        {
        }
    }

    private void CancelCatch()
    {
        obiAttachment.target =  (CatchBall.transform);
        attachAimPos = false;
    }
    
private void HandleAttachment()
    {
        if (_catchAimTrans)
        {
            obiAttachment.target = _catchAimTrans;
            attachAimPos = true;
        }
        else
        {
            attachAimPos = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        _catchAimTrans = other.transform;
    }

}