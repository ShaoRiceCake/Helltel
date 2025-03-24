using UnityEngine;
using Obi;

public class CatchTool : MonoBehaviour
{
    public GameObject m_ignoredFatherObject;
    public GameObject m_catchBall;
    public ObiParticleAttachment obiAttachment; 

    [HideInInspector]
    public bool attchAimPos = false;
    public bool isCacthing = true;

    private SphereCollider sphereCollider;
    private Transform aimTrans;

    private void Start()
    {
        m_catchBall = this.gameObject;

        sphereCollider = m_catchBall.GetComponent<SphereCollider>();

        // ≥ı ºªØ ObiParticleAttachment
        if (obiAttachment == null)
        {
            obiAttachment = m_catchBall.GetComponent<ObiParticleAttachment>();
            if (obiAttachment == null)
            {
                Debug.LogError("ObiParticleAttachment component is missing on CatchBall!");
            }
        }
    }

    private void Update()
    {
        if (isCacthing)
        {
            OnTriggerEnter(sphereCollider);
            HandleAttachment();
        }
        else
        {
            CancelCatch();
        }
    }

    public void CancelCatch()
    {
        obiAttachment.target =  (m_catchBall.transform);
        attchAimPos = false;
    }
    
private void HandleAttachment()
    {
        if (aimTrans != null)
        {
            obiAttachment.target = aimTrans;
            attchAimPos = true;
        }
        else
        {
            attchAimPos = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        aimTrans = other.transform;
    }

}