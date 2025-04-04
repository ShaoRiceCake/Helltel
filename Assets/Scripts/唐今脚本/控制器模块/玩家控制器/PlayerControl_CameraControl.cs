using UnityEngine;

public class PlayerControl_CameraControl : PlayerControl_BaseControl
{
    public float rotationSpeed = 100f; 
    public float minXAngle = -35f; 
    public float maxXAngle = 35f; 
    public GameObject aimObject;
    public GameObject virtualCamera;
    private readonly float _currentXRotation = 0f;

    protected override void Start()
    {
        virtualCamera.SetActive(IsLocalPlayer);
        forwardObject.SetActive(IsLocalPlayer);//根据是否本地玩家决定开启相机
    }

    private void Update()
    {
        // 水平旋转（左右）
        if (Input.GetKey(KeyCode.A))
        {
            aimObject.transform.GetChild(0).Rotate(0, -rotationSpeed * Time.deltaTime, 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            aimObject.transform.GetChild(0).Rotate(0, rotationSpeed * Time.deltaTime, 0);
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
        var currentRotation = aimObject.transform.localRotation;
        var newRotation = Quaternion.Euler(_currentXRotation, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z);

        aimObject.transform.localRotation = newRotation;
    }
}