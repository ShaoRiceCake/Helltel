using UnityEngine;

public class CatchControl : MonoBehaviour
{
    private float m_sphereRadius; // ��İ뾶
    public Vector3 SphereCenter { get; set; }
    private GameObject m_ignoredFatherObject;


    public GameObject IgnoredFatherObject
    {
        set
        {
            if (!value)
            {
                Debug.LogWarning("Catch Ball Not Set Any Father Object!");
                value = null;
            }
            else
            {
                m_ignoredFatherObject = value;
            }
        }

    }

    public float SphereRadius
    {
        get => m_sphereRadius;
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


    private void Update()
    {
        // �������ĵ�λ�ã����磬����ĳ��������ֶ����ƣ�
        SphereCenter = transform.position;

        // ������ڲ������岢�ҵ�������ĵĵ㣨�����ȼ��ͺ��������жϣ�
        Vector3 closestPoint = GetClosestPointInSphereWithPriority();

        if (closestPoint != Vector3.zero)
        {
            Debug.Log("������ĵĵ�: " + closestPoint);
        }
    }

    private Vector3 GetClosestPointInSphereWithPriority()
    {
        // ��ȡ�����ڵ�������ײ��
        Collider[] hitColliders = Physics.OverlapSphere(SphereCenter, m_sphereRadius);

        Vector3 closestPoint = Vector3.zero;
        float closestDistance = float.MaxValue;
        bool hasHighPriorityObject = false; // �Ƿ��⵽�����ȼ�����

        foreach (var hitCollider in hitColliders)
        {
            // �����ײ������Ҫ���Ե�������������壬������
            if (IsColliderIgnored(hitCollider))
            {
                continue;
            }

            // ����Ѿ���⵽�����ȼ����壬�ҵ�ǰ�����ǵ����ȼ���tagΪ"Floor"����������
            if (hasHighPriorityObject && hitCollider.CompareTag("Floor"))
            {
                continue;
            }

            // ��ȡ��ײ����������ĵĵ�
            Vector3 closestPointOnCollider = hitCollider.ClosestPoint(SphereCenter);

            // ����õ㵽���ĵľ���
            float distance = Vector3.Distance(closestPointOnCollider, SphereCenter);

            // ���������֮ǰ��¼�ĵ����������������;���
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = closestPointOnCollider;

                // �����ǰ���岻�ǵ����ȼ���tag����"Floor"�������Ϊ��⵽�����ȼ�����
                if (!hitCollider.CompareTag("Floor"))
                {
                    hasHighPriorityObject = true;
                }
            }
        }

        return closestPoint;
    }

    private bool IsColliderIgnored(Collider collider)
    {
        // ���δ������Ҫ���Ե����壬�򷵻�false
        if (m_ignoredFatherObject == null)
        {
            return false;
        }

        // �����ײ���Ƿ�����Ҫ���Ե��������������
        return collider.transform == m_ignoredFatherObject.transform || collider.transform.IsChildOf(m_ignoredFatherObject.transform);
    }

}