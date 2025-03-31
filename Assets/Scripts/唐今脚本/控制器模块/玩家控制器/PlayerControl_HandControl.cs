using UnityEngine;
using Obi;
using static PlayerControlInformationProcess;

public abstract class PlayerControl_HandControl : PlayerControl_BaseControl
{
    public GameObject handPrepareObj;
    public ObiParticleAttachment handControlAttachment;
    public float handMoveSpeed = 4.0f;
    public float cylinderRadius = 10.0f;
    public float cylinderHalfHeight = 7.0f;
    public float mouseSensitivity = 10f;
    public CatchTool  CatchTool;

    protected GameObject HandBallPrefab;
    protected int CurrentPlayerHand;
    protected GameObject HandObject;
        
    private bool _isMouseDown;
    private ControlBallGenerator _controlBallGenerator;


    public int CurrentHand
    {
        get => CurrentPlayerHand;
        set
        {
            if(value is < 0 or > 2)
            {
                Debug.LogError("Unknown CurrentPlayerHand!");
                CurrentPlayerHand = 0;
            }
            else
            {
                CurrentPlayerHand = value;
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        
        _controlBallGenerator = gameObject.AddComponent<ControlBallGenerator>();
        
        HandBallPrefab = ControlBallGenerator.GenerateControlBall();
        
        if (handPrepareObj != null) NullCheckerTool.CheckNull(handPrepareObj,CatchTool, HandBallPrefab, handControlAttachment);
        
        SubscribeEvents();
    }


    protected virtual void OnDestroy()
    {
        if (controlHandler != null)
        {
            UnsubscribeEvents();
        }
    }

    private void SubscribeEvents()
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
        if(controlHandler.mCurrentControlMode == ControlMode.LegControl)
        {
            CurrentHand = 0;
        }
    }

    private void OnCancelHandGrab()
    {
        CurrentHand = 0;
    }
    protected void OnLiftLeftHand()
    {
        CurrentHand = 1;
        _isMouseDown = true;

    }

    protected void OnReleaseLeftHand()
    {
        CurrentHand = 1;
        _isMouseDown = false;

    }
    protected void OnLiftRightHand()
    {
        CurrentHand = 2;
        _isMouseDown = true;

    }

    protected void OnReleaseRightHand()
    {
        CurrentHand = 2;
        _isMouseDown = false;

    }

    private void OnMouseMove(Vector2 mouseDelta)
    {
        if (CurrentPlayerHand == 0 || HandObject == null || handPrepareObj == null)
            return;

        var mouseX = mouseDelta.x * mouseSensitivity;
        var mouseY = mouseDelta.y * mouseSensitivity;
        
        var localMovement = Vector3.zero;

        if (_isMouseDown)
        {
            localMovement = new Vector3(mouseX, mouseY, 0) * handMoveSpeed;
        }
        else
        {
            localMovement = new Vector3(mouseX, 0, mouseY) * handMoveSpeed;
        }

        var forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; 
        forwardDirection.Normalize();

        var rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        var worldMovement = forwardDirection * localMovement.z + rightDirection * localMovement.x;
        worldMovement.y = localMovement.y;

        var newWorldPosition = HandObject.transform.position + worldMovement * Time.deltaTime;

        var newLocalPosition = handPrepareObj.transform.InverseTransformPoint(newWorldPosition);

        ApplyCylinderConstraint(ref newLocalPosition);

        HandObject.transform.position = handPrepareObj.transform.TransformPoint(newLocalPosition);
    }

    private void ApplyCylinderConstraint(ref Vector3 position)
    {
        var horizontal = new Vector2(position.x, position.z);
        if (horizontal.magnitude > cylinderRadius)
        {
            horizontal = horizontal.normalized * cylinderRadius;
            position.x = horizontal.x;
            position.z = horizontal.y;
        }
        position.y = Mathf.Clamp(position.y, -cylinderHalfHeight, cylinderHalfHeight);
    }
}
