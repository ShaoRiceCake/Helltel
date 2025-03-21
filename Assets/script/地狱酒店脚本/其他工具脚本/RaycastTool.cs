using UnityEngine;

[ExecuteInEditMode] // 允许在编辑模式下运行
public class RaycastTool : MonoBehaviour
{
    [Header("射线设置")]
    private float rayLength = 5f; // 射线长度
    private Vector3 rayDirection = Vector3.down; // 射线方向
    public LayerMask ignoreLayers; // 需要忽略的Layer
    public GameObject rayLauncher;

    [Header("调试")]
    public bool showDebug = false; // 是否显示调试信息
    public Color rayColor = Color.green; // 射线颜色
    public Color hitPointColor = Color.red; // 碰撞点颜色

    private RaycastHit hitInfo; // 射线碰撞信息
    private bool isHit = false; // 是否检测到碰撞

    private void Start()
    {
        if (!rayLauncher)
        {
            Debug.LogWarning("RayLauncher is null!");
        }
    }

    public float RayLength
    {
        get => rayLength;
        set
        {
            if (rayLength <= 0)
            {
                Debug.LogWarning("RayLength is zero!");
                rayLength = value;
            }
            else
            {
                rayLength = value;
            }
        }
    }

    public Vector3 RyDirection
    {
        get => rayDirection;
        set
        {
            if (RyDirection == Vector3.zero)
            {
                Debug.LogWarning("RyDirection is zero!");
                rayDirection = value;
            }
            else
            {
                rayDirection = value;
            }
        }
    }


    void Update()
    {
        PerformRaycast();
    }

    void PerformRaycast()
    {
        // 计算射线的起点和方向
        Vector3 rayOrigin;
        Vector3 direction;
        if (rayLauncher)
        {
            rayOrigin = rayLauncher.transform.position;
            direction = rayLauncher.transform.TransformDirection(rayDirection.normalized);

        }
        else
        {
            rayOrigin = transform.position;
            direction = transform.TransformDirection(rayDirection.normalized);
        }

        // 执行射线检测
        isHit = Physics.Raycast(rayOrigin, direction, out hitInfo, rayLength, ~ignoreLayers);
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;

        // 绘制射线
        Gizmos.color = rayColor;
        Vector3 rayOrigin = transform.position;
        Vector3 direction = transform.TransformDirection(rayDirection.normalized);
        Gizmos.DrawLine(rayOrigin, rayOrigin + direction * rayLength);

        // 如果检测到碰撞，绘制碰撞点
        if (isHit)
        {
            Gizmos.color = hitPointColor;
            Gizmos.DrawSphere(hitInfo.point, 0.1f);
        }
    }

    // 获取碰撞点（如果没有碰撞，返回射线的终点）
    public Transform GetHitTrans()
    {
        if (isHit)
        {
            return hitInfo.transform;
        }
        else
        {
            return null;
        }
    }

    // 获取碰撞点（如果没有碰撞，返回射线的终点）
    public Vector3 GetHitPoint()
    {
        if (isHit)
        {
            return hitInfo.point;
        }
        else
        {
            return Vector3.zero;
        }
    }



    // 获取是否检测到碰撞
    public bool IsHit()
    {
        return isHit;
    }

    // 获取碰撞信息
    public RaycastHit GetHitInfo()
    {
        return hitInfo;
    }
}