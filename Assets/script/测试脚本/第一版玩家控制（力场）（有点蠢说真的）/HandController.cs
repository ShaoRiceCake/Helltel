using UnityEngine;

public class HandController : MonoBehaviour
{
    public GameObject mouseObj;
    public GameObject ecoObj;
    public float mouseSensitivity = 0.01f;
    public float limit = 3;

    private Vector3 ecoMousePos;
    private Vector3 relativePos;
    private bool verticalControlMode;
    private Vector3 newPosition;

    private void Start()
    {
        mouseObj.transform.position = ecoObj.transform.position;
        ecoMousePos = Input.mousePosition;
        verticalControlMode = false;
        newPosition = ecoObj.transform.position;
    }

    private void Update()
    {
        CalculateRelativeDisplacement(); // 每帧先计算相对位移
        SwitchControlMode();
        Move();
    }

    private void CalculateRelativeDisplacement()
    {
        Vector3 currentMousePos = Input.mousePosition;
        relativePos = currentMousePos - ecoMousePos;
        ecoMousePos = currentMousePos; // 更新为当前帧的鼠标位置
    }

    private void SwitchControlMode()
    {
        if (Input.GetMouseButtonDown(1))
        {
            verticalControlMode = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            verticalControlMode = false;
        }
    }

    private void Move()
    {
        Vector3 displacement = Vector3.zero;

        if (!verticalControlMode) // XZ平面移动
        {
            displacement.x = relativePos.x * mouseSensitivity;
            displacement.z = relativePos.y * mouseSensitivity;
        }
        else // XY平面移动
        {
            displacement.x = relativePos.x * mouseSensitivity;
            displacement.y = relativePos.y * mouseSensitivity;
        }

        // 应用增量并限制范围
        newPosition = mouseObj.transform.position + displacement;
        newPosition.x = Mathf.Clamp(newPosition.x, -limit, limit);
        newPosition.y = Mathf.Clamp(newPosition.y, -limit, limit);
        newPosition.z = Mathf.Clamp(newPosition.z, -limit, limit);

        mouseObj.transform.position = newPosition;
    }
}