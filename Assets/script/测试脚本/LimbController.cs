using UnityEngine;
using Obi;

public class SoftbodyHandController : MonoBehaviour
{
    [Header("Bindings")]
    public Transform chestPoint;        // 胸口参考点
    public Transform leftHand;         // 左手绑定对象
    public ObiSolver solver;           // Obi 粒子求解器
    public float moveSpeed = 8f;       // 移动速度
    public float maxRadius = 1.5f;     // 最大活动半径
    public float particlePickRadius = 0.1f; // 拾取粒子的最大距离

    private int controlledParticleIndex = -1; // 当前控制的粒子索引
    private Vector3 targetPosition;           // 目标位置
    private Plane ecoPlane;                   // 当前活动平面
    private Vector3 planeOrigin;              // 平面原点
    private bool isRightMouseDown;            // 右键是否按下

    void Start()
    {
        InitializeEcoPlane();
    }

    void Update()
    {
        HandleInput();
        UpdateEcoPlane();

        // 如果没有控制粒子，尝试找到手部位置附近的粒子
        if (controlledParticleIndex == -1)
        {
            controlledParticleIndex = FindNearestParticle(leftHand.position);
        }

        // 如果有控制的粒子，更新其位置
        if (controlledParticleIndex != -1)
        {
            UpdateParticlePosition();
        }

        print(controlledParticleIndex);
    }

    private void InitializeEcoPlane()
    {
        // 初始化平面：胸口高度，角色局部XZ平面
        planeOrigin = chestPoint.position;
        ecoPlane = new Plane(transform.up, planeOrigin);
    }

    private void HandleInput()
    {
        isRightMouseDown = Input.GetMouseButton(1);
    }

    private void UpdateEcoPlane()
    {
        if (isRightMouseDown)
        {
            // 右键按下：使用垂直平面
            Vector3 planeNormal = transform.forward;
            planeOrigin = leftHand.position;
            ecoPlane = new Plane(planeNormal, planeOrigin);
        }
        else
        {
            // 右键松开：使用水平平面
            planeOrigin = new Vector3(
                chestPoint.position.x,
                leftHand.position.y,
                chestPoint.position.z
            );
            ecoPlane = new Plane(transform.up, planeOrigin);
        }
    }

    private int FindNearestParticle(Vector3 position)
    {
        int nearestIndex = -1;
        float nearestDistance = float.MaxValue;

        // 遍历所有粒子，找到距离手部位置最近的粒子
        for (int i = 0; i < solver.renderablePositions.count; ++i)
        {
            Vector3 particlePosition = solver.transform.TransformPoint(solver.renderablePositions[i]);
            float distance = Vector3.Distance(particlePosition, position);

            if (distance < particlePickRadius && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private void UpdateParticlePosition()
    {
        // 将鼠标位置映射到活动平面
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter;

        if (ecoPlane.Raycast(ray, out enter))
        {
            // 获取平面交点
            Vector3 hitPoint = ray.GetPoint(enter);

            // 计算局部坐标
            Vector3 localOffset = transform.InverseTransformPoint(hitPoint) -
                                 transform.InverseTransformPoint(planeOrigin);

            // 限制活动半径
            localOffset = Vector3.ClampMagnitude(localOffset, maxRadius);

            // 转换回世界坐标
            targetPosition = transform.TransformPoint(
                transform.InverseTransformPoint(planeOrigin) + localOffset
            );
        }

        // 将目标位置应用到粒子
        solver.renderablePositions[controlledParticleIndex] = solver.transform.InverseTransformPoint(targetPosition);
    }
}