using UnityEngine;
using Obi;
using static PlayerControlInformationProcess;

public abstract class PlayerControl_HandControl : PlayerControl_BaseControl
{
    public GameObject handPrepareObj;

    public ObiParticleAttachment handControlAttachment;

    public float handMoveSpeed = 4.0f;
    public float cylinderRadius = 9.0f;
    public float cylinderHalfHeight = 6.0f;
    public float mouseSensitivity = 10f;

    public GameObject handBallPrefab;

    protected int currentHand = 0;

    protected GameObject handObject;

    protected bool isMouseDown = false;

    public int CurrentHand
    {
        // 0是未控制任意一条手臂，1是左臂，2是右臂

        get => currentHand;
        set
        {
            if(value < 0 || value > 2)
            {
                Debug.LogError("Unknown currentHand!");
                currentHand = 0;
            }
            else
            {
                currentHand = value;
            }
        }
    }

    protected override void Start()
    {
        base.Start();

        NullCheckerTool.CheckNull(handPrepareObj, handBallPrefab, handControlAttachment);

        SubscribeEvents();
    }


    protected virtual void OnDestroy()
    {
        if (controlHandler != null)
        {
            UnsubscribeEvents();
        }
    }

    protected virtual void SubscribeEvents()
    {
        controlHandler.onLiftLeftHand.AddListener(OnLiftLeftHand);
        controlHandler.onReleaseLeftHand.AddListener(OnReleaseLeftHand);
        controlHandler.onLiftRightHand.AddListener(OnLiftRightHand);
        controlHandler.onReleaseRightHand.AddListener(OnReleaseRightHand);
        controlHandler.onCancelHandGrab.AddListener(OnCancelHandGrab);
        controlHandler.onMouseMoveUpdate.AddListener(OnMouseMove);

    }

    protected virtual void UnsubscribeEvents()
    {
        controlHandler.onCancelHandGrab.RemoveListener(OnCancelHandGrab);
        controlHandler.onMouseMoveUpdate.RemoveListener(OnMouseMove);

    }

    protected virtual void Update()
    {
        if(controlHandler.m_currentControlMode == ControlMode.LegControl)
        {
            CurrentHand = 0;
        }
    }
  
    protected void OnCancelHandGrab()
    {
        CurrentHand = 0;
    }
    protected virtual void OnLiftLeftHand()
    {
        CurrentHand = 1;
        isMouseDown = true;

    }

    protected virtual void OnReleaseLeftHand()
    {
        CurrentHand = 1;
        isMouseDown = false;

    }
    protected virtual void OnLiftRightHand()
    {
        CurrentHand = 2;
        isMouseDown = true;

    }

    protected virtual void OnReleaseRightHand()
    {
        CurrentHand = 2;
        isMouseDown = false;

    }

    protected void OnMouseMove(Vector2 mouseDelta)
    {
        if (currentHand == 0 || handObject == null || handPrepareObj == null)
            return;

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;


        Vector3 localMovement = Vector3.zero;

        if (isMouseDown)
        {
            localMovement = new Vector3(mouseX, mouseY, 0) * handMoveSpeed;
        }
        else
        {
            localMovement = new Vector3(mouseX, 0, mouseY) * handMoveSpeed;
        }

        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; 
        forwardDirection.Normalize();

        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        Vector3 worldMovement = forwardDirection * localMovement.z + rightDirection * localMovement.x;
        worldMovement.y = localMovement.y;

        Vector3 newWorldPosition = handObject.transform.position + worldMovement * Time.deltaTime;

        Vector3 newLocalPosition = handPrepareObj.transform.InverseTransformPoint(newWorldPosition);

        ApplyCylinderConstraint(ref newLocalPosition);

        handObject.transform.position = handPrepareObj.transform.TransformPoint(newLocalPosition);
    }

    protected void ApplyCylinderConstraint(ref Vector3 position)
    {
        Vector2 horizontal = new Vector2(position.x, position.z);
        if (horizontal.magnitude > cylinderRadius)
        {
            horizontal = horizontal.normalized * cylinderRadius;
            position.x = horizontal.x;
            position.z = horizontal.y;
        }
        position.y = Mathf.Clamp(position.y, -cylinderHalfHeight, cylinderHalfHeight);
    }
}
