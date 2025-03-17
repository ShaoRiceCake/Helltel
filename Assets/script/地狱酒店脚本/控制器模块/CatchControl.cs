using UnityEngine;

public class CatchControl : MonoBehaviour
{
    private float m_sphereRadius; // 球的半径
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
        // 更新球心的位置（例如，跟随某个对象或手动控制）
        SphereCenter = transform.position;

        // 检测球内部的物体并找到最靠近球心的点（带优先级和忽略物体判断）
        Vector3 closestPoint = GetClosestPointInSphereWithPriority();

        if (closestPoint != Vector3.zero)
        {
            Debug.Log("最靠近球心的点: " + closestPoint);
        }
    }

    private Vector3 GetClosestPointInSphereWithPriority()
    {
        // 获取球体内的所有碰撞体
        Collider[] hitColliders = Physics.OverlapSphere(SphereCenter, m_sphereRadius);

        Vector3 closestPoint = Vector3.zero;
        float closestDistance = float.MaxValue;
        bool hasHighPriorityObject = false; // 是否检测到高优先级物体

        foreach (var hitCollider in hitColliders)
        {
            // 如果碰撞体是需要忽略的物体或其子物体，则跳过
            if (IsColliderIgnored(hitCollider))
            {
                continue;
            }

            // 如果已经检测到高优先级物体，且当前物体是低优先级（tag为"Floor"），则跳过
            if (hasHighPriorityObject && hitCollider.CompareTag("Floor"))
            {
                continue;
            }

            // 获取碰撞体上最靠近球心的点
            Vector3 closestPointOnCollider = hitCollider.ClosestPoint(SphereCenter);

            // 计算该点到球心的距离
            float distance = Vector3.Distance(closestPointOnCollider, SphereCenter);

            // 如果这个点比之前记录的点更近，则更新最近点和距离
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = closestPointOnCollider;

                // 如果当前物体不是低优先级（tag不是"Floor"），标记为检测到高优先级物体
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
        // 如果未设置需要忽略的物体，则返回false
        if (m_ignoredFatherObject == null)
        {
            return false;
        }

        // 检查碰撞体是否是需要忽略的物体或其子物体
        return collider.transform == m_ignoredFatherObject.transform || collider.transform.IsChildOf(m_ignoredFatherObject.transform);
    }

}