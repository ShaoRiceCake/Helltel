using UnityEngine;

/// <summary>
/// 一个功能全面的轨道摄像机控制器。
/// - 按住鼠标右键并移动：围绕目标旋转。
/// - 滚动鼠标中键：缩放。
/// - 按住鼠标中键并移动：平移。
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [Header("目标与距离设置")]
    [Tooltip("摄像机观察的目标")]
    public Transform target; // 摄像机观察的目标

    [Tooltip("摄像机与目标的初始距离")]
    public float distance = 5.0f; // 初始距离

    [Tooltip("允许的最小和最大距离")]
    public float minDistance = 1.0f;
    public float maxDistance = 15.0f;

    [Header("速度与平滑度设置")]
    [Tooltip("旋转速度")]
    public float rotationSpeed = 120.0f;

    [Tooltip("缩放速度")]
    public float zoomSpeed = 5.0f;

    [Tooltip("平移速度")]
    public float panSpeed = 0.5f;

    [Tooltip("位置移动的平滑度 (数值越小越平滑)")]
    public float positionDamping = 0.15f;

    [Tooltip("旋转的平滑度 (数值越小越平滑)")]
    public float rotationDamping = 0.15f;


    [Header("角度限制")]
    [Tooltip("垂直方向的最小角度 (X轴旋转)")]
    public float yMinLimit = -20f;
    [Tooltip("垂直方向的最大角度 (X轴旋转)")]
    public float yMaxLimit = 80f;


    // 私有变量
    private float x = 0.0f;
    private float y = 0.0f;

    private Quaternion currentRotation;
    private Vector3 currentPosition;
    private Quaternion desiredRotation;
    private Vector3 desiredPosition;

    private Vector3 targetOffset = Vector3.zero;

    void Start()
    {
        // 初始化摄像机位置和旋转
        if (target == null)
        {
            // 如果没有指定目标，自动创建一个
            GameObject targetGO = new GameObject("Camera Target");
            targetGO.transform.position = transform.position + transform.forward * distance;
            target = targetGO.transform;
        }

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (target)
        {
            // 轨道旋转 (鼠标右键)
            if (Input.GetMouseButton(1))
            {
                x += Input.GetAxis("Mouse X") * rotationSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y") * rotationSpeed * 0.02f;
                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }

            // 缩放 (鼠标滚轮)
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, minDistance, maxDistance);
            
            // 计算期望的旋转
            desiredRotation = Quaternion.Euler(y, x, 0);

            // 计算期望的位置
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            desiredPosition = desiredRotation * negDistance + target.position + targetOffset;

            // 平滑处理
            currentRotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationDamping);
            currentPosition = Vector3.Lerp(transform.position, desiredPosition, positionDamping);
            
            transform.rotation = currentRotation;
            transform.position = currentPosition;

            // 平移 (鼠标中键)
            if (Input.GetMouseButton(2))
            {
                // 根据摄像机的方向计算平移量
                Vector3 forward = transform.forward;
                Vector3 right = transform.right;

                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                Vector3 pan = (-right * (Input.GetAxis("Mouse X") * panSpeed)) + (-forward * (Input.GetAxis("Mouse Y") * panSpeed));
                target.position += pan;
                targetOffset += pan; // 平移时，需要更新目标点的偏移量
            }
        }
    }

    /// <summary>
    /// 将角度限制在 min 和 max 之间
    /// </summary>
    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}