using UnityEngine;

public class PlayerController_Foot : PlayerController_Base
{
    [Header("�������")]
    public float rotationForce = 150f;
    public float dampingForce = 3f;
    public float followDistance = 5f;

    private Rigidbody rb;
    private Transform target;
    private GameObject activeObj;

    protected override void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePosition;
        rb.maxAngularVelocity = 20f;
    }

    protected override void HandleInput()
    {
        // ԭ�����봦���߼�
    }

    protected override void ApplyMovement()
    {
        Vector3 worldDelta = Camera.main.transform.right * mouseDelta.x +
                           Camera.main.transform.up * mouseDelta.y;
        worldDelta *= mouseSensitivity;

        Vector3 newPos = transform.position + worldDelta;
        newPos.y = transform.position.y;
        transform.position = newPos;

        ApplyRotation();
    }

    private void ApplyRotation()
    {
        Vector3 targetDirection = transform.position - target.position;
        if (targetDirection == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion rotationDelta = targetRotation * Quaternion.Inverse(target.rotation);

        rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);

        Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad * rotationForce);
        angularVelocity -= rb.angularVelocity * dampingForce;

        rb.AddTorque(angularVelocity, ForceMode.Acceleration);
    }
}
