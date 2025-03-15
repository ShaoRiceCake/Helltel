using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float rotationSpeed = 100f; // 旋转速度
    public float minXAngle = -35f; // 绕X轴旋转的最小角度
    public float maxXAngle = 35f;  // 绕X轴旋转的最大角度

    public float zoomSpeed = 10f; // 视角拉近拉远的速度
    public float minZoomDistance = 2f; // 最近视角距离
    public float maxZoomDistance = 10f; // 最远视角距离
    public float zoomSmoothTime = 0.2f; // 视角缩放的平滑时间

    private float currentXRotation = 0f; // 当前绕X轴的旋转角度
    private Vector3 initialLocalPosition; // 摄像机初始的局部位置
    private float targetZoomDistance; // 目标视角距离
    private float currentZoomVelocity; // 当前视角缩放的插值速度

    void Start()
    {
        // 记录摄像机初始的局部位置
        initialLocalPosition = transform.localPosition;
        // 初始化目标视角距离为当前距离
        targetZoomDistance = -transform.localPosition.z;
    }

    void Update()
    {
        // 按下A键，绕Y轴逆时针旋转
        if (Input.GetKey(KeyCode.A))
        {
            transform.parent.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
        }

        // 按下D键，绕Y轴顺时针旋转
        if (Input.GetKey(KeyCode.D))
        {
            transform.parent.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }

        // 按下W键，绕X轴逆时针旋转
        if (Input.GetKey(KeyCode.W))
        {
            float deltaRotation = -rotationSpeed * Time.deltaTime;
            currentXRotation += deltaRotation;
            currentXRotation = Mathf.Clamp(currentXRotation, minXAngle, maxXAngle); // 限制角度
            transform.localEulerAngles = new Vector3(currentXRotation, 0, 0);
        }

        // 按下S键，绕X轴顺时针旋转
        if (Input.GetKey(KeyCode.S))
        {
            float deltaRotation = rotationSpeed * Time.deltaTime;
            currentXRotation += deltaRotation;
            currentXRotation = Mathf.Clamp(currentXRotation, minXAngle, maxXAngle); // 限制角度
            transform.localEulerAngles = new Vector3(currentXRotation, 0, 0);
        }

        //// 处理滚轮拉近拉远视角
        //HandleZoom();
    }

    void HandleZoom()
    {
        // 获取鼠标滚轮的输入
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            // 计算目标Z轴位置
            targetZoomDistance += -scrollInput * zoomSpeed;
            // 限制目标Z轴位置在最近和最远视角之间
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoomDistance, maxZoomDistance);
        }

        // 使用插值平滑地调整摄像机的位置
        float newZ = Mathf.SmoothDamp(-transform.localPosition.z, targetZoomDistance, ref currentZoomVelocity, zoomSmoothTime);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -newZ);
    }
}