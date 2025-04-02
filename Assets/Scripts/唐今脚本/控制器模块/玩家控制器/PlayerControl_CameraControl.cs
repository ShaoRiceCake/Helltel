using UnityEngine;

public class PlayerControl_CameraControl : PlayerControl_BaseControl
{
    public float rotationSpeed = 100f; 
    public float minXAngle = -35f; 
    public float maxXAngle = 35f; 

    private float _currentXRotation = 0f;

    protected override void Start()
    {
        forwardObject.SetActive(IsLocalPlayer);//根据是否本地玩家决定开启相机
    }

    private void Update()
    {
        if (!GameManager.instance.isGameing) return;
        if (!IsLocalPlayer) return;

        // 水平旋转（左右）
        if (Input.GetKey(KeyCode.A))
        {
            transform.parent.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.parent.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }

        // // 垂直旋转（上下）
        // if (Input.GetKey(KeyCode.W))
        // {
        //     _currentXRotation -= rotationSpeed * Time.deltaTime;
        //     _currentXRotation = Mathf.Clamp(_currentXRotation, minXAngle, maxXAngle);
        //     ApplyRotation();
        // }
        //
        // if (!Input.GetKey(KeyCode.S)) return;
        // _currentXRotation += rotationSpeed * Time.deltaTime;
        // _currentXRotation = Mathf.Clamp(_currentXRotation, minXAngle, maxXAngle);
        // ApplyRotation();
    }

    private void ApplyRotation()
    {
        // 获取当前的局部旋转
        var currentRotation = transform.localRotation;

        // 计算新的旋转角度
        var newRotation = Quaternion.Euler(_currentXRotation, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z);

        // 应用新的旋转
        transform.localRotation = newRotation;
    }
}