using UnityEngine;

[ExecuteInEditMode] // �����ڱ༭ģʽ������
public class RaycastTool : MonoBehaviour
{
    [Header("��������")]
    private float rayLength = 5f; // ���߳���
    private Vector3 rayDirection = Vector3.down; // ���߷���
    public LayerMask ignoreLayers; // ��Ҫ���Ե�Layer
    public GameObject rayLauncher;

    [Header("����")]
    public bool showDebug = false; // �Ƿ���ʾ������Ϣ
    public Color rayColor = Color.green; // ������ɫ
    public Color hitPointColor = Color.red; // ��ײ����ɫ

    private RaycastHit hitInfo; // ������ײ��Ϣ
    private bool isHit = false; // �Ƿ��⵽��ײ

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
        // �������ߵ����ͷ���
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

        // ִ�����߼��
        isHit = Physics.Raycast(rayOrigin, direction, out hitInfo, rayLength, ~ignoreLayers);
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;

        // ��������
        Gizmos.color = rayColor;
        Vector3 rayOrigin = transform.position;
        Vector3 direction = transform.TransformDirection(rayDirection.normalized);
        Gizmos.DrawLine(rayOrigin, rayOrigin + direction * rayLength);

        // �����⵽��ײ��������ײ��
        if (isHit)
        {
            Gizmos.color = hitPointColor;
            Gizmos.DrawSphere(hitInfo.point, 0.1f);
        }
    }

    // ��ȡ��ײ�㣨���û����ײ���������ߵ��յ㣩
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

    // ��ȡ��ײ�㣨���û����ײ���������ߵ��յ㣩
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



    // ��ȡ�Ƿ��⵽��ײ
    public bool IsHit()
    {
        return isHit;
    }

    // ��ȡ��ײ��Ϣ
    public RaycastHit GetHitInfo()
    {
        return hitInfo;
    }
}