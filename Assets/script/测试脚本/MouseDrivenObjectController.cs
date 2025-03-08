using UnityEngine;

public class MouseDrivenObjectController : MonoBehaviour
{
    [Header("Bindings")]
    public Transform chestPoint;        // 胸口参考点
    public Transform leftHand;          // 左手绑定对象
    public float moveSpeed = 8f;        // 移动速度
    public float maxRadius = 1.5f;      // 最大活动半径

    [Header("Circle Rendering")]
    public Material circleMaterial;     // 圆形材质
    public float lineWidth = 0.05f;     // 线条宽度

    private Camera mainCamera;
    private Plane ecoPlane;             // 当前活动平面
    private Vector3 planeOrigin;        // 平面原点
    private Vector3 targetPosition;     // 目标位置
    private bool isRightMouseDown;      // 右键是否按下
    private bool isHandControl = true; // 当前控制模式（默认手部控制）
    private Vector3 startPos;           // 平面原点
    private LineRenderer circleRenderer; // 用于绘制圆形的LineRenderer

    void Start()
    {
        mainCamera = Camera.main;
        InitializeEcoPlane();
        ResetHandPosition();
        startPos = transform.position;

        // 初始化圆形渲染器
        InitializeCircleRenderer();
    }

    void Update()
    {
        HandleInput();

        if (isHandControl)
        {
            UpdateEcoPlane();
            UpdateHandPosition();
        }

        // 更新圆形的位置和形状
        UpdateCircleRenderer();
    }

    private void InitializeEcoPlane()
    {
        planeOrigin = chestPoint.position;
        ecoPlane = new Plane(transform.up, planeOrigin);
    }

    private void HandleInput()
    {
        isRightMouseDown = Input.GetMouseButton(1);

        // 鼠标中键切换控制模式
        if (Input.GetMouseButtonDown(2))
        {
            isHandControl = !isHandControl;

            if (isHandControl)
            {
                // 切换回手部控制时立即归位
                ResetHandPosition();
            }
        }
    }

    private void UpdateEcoPlane()
    {
        if (isRightMouseDown)
        {
            Vector3 planeNormal = transform.forward;
            planeOrigin = leftHand.position;
            ecoPlane = new Plane(planeNormal, planeOrigin);
        }
        else
        {
            planeOrigin = new Vector3(
                chestPoint.position.x,
                leftHand.position.y,
                chestPoint.position.z
            );
            ecoPlane = new Plane(transform.up, planeOrigin);
        }
    }

    private void UpdateHandPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float enter;

        if (ecoPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 localOffset = transform.InverseTransformPoint(hitPoint) -
                                 transform.InverseTransformPoint(planeOrigin);

            localOffset = Vector3.ClampMagnitude(localOffset, maxRadius);

            targetPosition = transform.TransformPoint(
                transform.InverseTransformPoint(planeOrigin) + localOffset
            );
        }

        leftHand.position = Vector3.Lerp(
            leftHand.position,
            targetPosition,
            Time.deltaTime * moveSpeed
        );
    }

    private void ResetHandPosition()
    {
        leftHand.position = chestPoint.position;
        targetPosition = chestPoint.position;
    }

    private void InitializeCircleRenderer()
    {
        // 创建LineRenderer组件
        circleRenderer = gameObject.AddComponent<LineRenderer>();

        // 设置材质
        circleRenderer.material = circleMaterial;

        // 设置线条宽度
        circleRenderer.startWidth = lineWidth;
        circleRenderer.endWidth = lineWidth;

        // 设置圆形分段数
        int segments = 36;
        circleRenderer.positionCount = segments + 1; // 闭合圆形需要多一个点
    }

    private void UpdateCircleRenderer()
    {
        if (circleRenderer == null) return;

        int segments = circleRenderer.positionCount - 1;
        float anglePerSegment = 360f / segments;

        // 更新圆形的位置
        for (int i = 0; i <= segments; i++)
        {
            float angle = anglePerSegment * i;
            Vector3 point = GetCirclePoint(angle);
            circleRenderer.SetPosition(i, point);
        }
    }

    private Vector3 GetCirclePoint(float angle)
    {
        // 将角度转换为弧度
        float radian = angle * Mathf.Deg2Rad;

        // 计算圆上的点
        float x = Mathf.Cos(radian) * maxRadius;
        float z = Mathf.Sin(radian) * maxRadius;

        // 将局部坐标转换为世界坐标
        return planeOrigin + transform.TransformDirection(new Vector3(x, 0, z));
    }
}