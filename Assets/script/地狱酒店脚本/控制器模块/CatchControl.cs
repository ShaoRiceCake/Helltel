using UnityEngine;
using Obi;

public class CatchControl : MonoBehaviour
{
    public GameObject m_ignoredFatherObject;
    public GameObject m_catchBall;
    public ObiParticleAttachment obiAttachment; // 新增 ObiParticleAttachment 组件

    [HideInInspector]
    public bool attchAimPos = false;

    private Transform aimTrans;
    private float m_sphereRadius;

    public float SphereRadius
    {
        set
        {
            if (value <= 0)
            {
                Debug.LogWarning("Catch SphereRadius Can Not Be Used!");
                m_sphereRadius = 0;
            }
            else
            {
                m_sphereRadius = value;
            }
        }
    }

    private void Start()
    {
        SphereCollider sphereCollider = m_catchBall.GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            m_sphereRadius = sphereCollider.radius;
        }
        else
        {
            Debug.LogError("CatchBall does not have a SphereCollider component!");
        }

        // 初始化 ObiParticleAttachment
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
        aimTrans = GetClosestTransformInSphereWithPriority();
        HandleAttachment();
    }

    private void HandleAttachment()
    {
        if (aimTrans != null)
        {
            obiAttachment.enabled = true;
            obiAttachment.target = aimTrans;
            attchAimPos = true;
        }
        else
        {
            obiAttachment.enabled = false;
            attchAimPos = false;
        }
    }

    private Transform GetClosestTransformInSphereWithPriority()
    {
        Collider[] hitColliders = Physics.OverlapSphere(m_catchBall.transform.position, m_sphereRadius);

        Transform closestTransform = null;
        float closestDistance = float.MaxValue;
        bool hasHighPriorityObject = false;

        foreach (var hitCollider in hitColliders)
        {
            if (IsColliderIgnored(hitCollider))
            {
                continue;
            }

            if (hasHighPriorityObject && hitCollider.CompareTag("Floor"))
            {
                continue;
            }

            Vector3 closestPointOnCollider = hitCollider.ClosestPoint(m_catchBall.transform.position);
            float distance = Vector3.Distance(closestPointOnCollider, m_catchBall.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTransform = hitCollider.transform;

                if (!hitCollider.CompareTag("Floor"))
                {
                    hasHighPriorityObject = true;
                }
            }
        }

        return closestTransform;
    }

    private bool IsColliderIgnored(Collider collider)
    {
        if (m_ignoredFatherObject == null)
        {
            return false;
        }

        return collider.transform == m_ignoredFatherObject.transform || collider.transform.IsChildOf(m_ignoredFatherObject.transform);
    }
}