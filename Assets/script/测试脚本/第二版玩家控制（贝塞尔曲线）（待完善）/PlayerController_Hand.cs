using UnityEngine;

public class PlayerController_Hand : PlayerController_Base
{
    private bool verticalControlMode;

    [Header("关联对象")]
    public GameObject mousePointer;

    protected override void InitializeComponents()
    {
        mousePointer.transform.position = transform.position;
        verticalControlMode = false;
    }

    protected override void HandleInput()
    {
        if (Input.GetMouseButtonDown(1)) verticalControlMode = true;
        if (Input.GetMouseButtonUp(1)) verticalControlMode = false;
    }

    protected override void ApplyMovement()
    {
        Vector3 displacement = verticalControlMode ?
            new Vector3(mouseDelta.x, mouseDelta.y, 0) * mouseSensitivity :
            new Vector3(mouseDelta.x, 0, mouseDelta.y) * mouseSensitivity;

        Vector3 newPos = mousePointer.transform.position + displacement;
        newPos = new Vector3(
            Mathf.Clamp(newPos.x, -limit, limit),
            Mathf.Clamp(newPos.y, -limit, limit),
            Mathf.Clamp(newPos.z, -limit, limit)
        );

        mousePointer.transform.position = newPos;
    }
}
