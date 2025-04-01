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
    public CatchTool  catchTool;
    public GameObject handBallPrefab;
    
    protected int CurrentPlayerHand;
    private bool _isMouseDown;
    protected bool IsHandActive;


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
        
        if (handPrepareObj != null) NullCheckerTool.CheckNull(handPrepareObj,catchTool, handBallPrefab, handControlAttachment);
        
        handBallPrefab.SetActive(false); 
        
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
        if (CurrentPlayerHand == 0 || handBallPrefab == null || handPrepareObj == null)
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

        var newWorldPosition = handBallPrefab.transform.position + worldMovement * Time.deltaTime;

        var newLocalPosition = handPrepareObj.transform.InverseTransformPoint(newWorldPosition);

        ApplyCylinderConstraint(ref newLocalPosition);

        handBallPrefab.transform.position = handPrepareObj.transform.TransformPoint(newLocalPosition);
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
    
    protected void ActivateControlBall()
    {
        if (!handBallPrefab) return;
    
        handBallPrefab.SetActive(true);
        handBallPrefab.transform.position = ObiGetGroupParticles.GetParticleWorldPositions(handControlAttachment)[0];
        handControlAttachment.enabled = true;
        handControlAttachment.target = handBallPrefab.transform;
        catchTool.CatchBall = handBallPrefab;
        IsHandActive = true;
    }
    protected void DeactivateControlBall()
    {
        if (!handBallPrefab) return;
    
        handBallPrefab.SetActive(false);
        handControlAttachment.enabled = false;
        handControlAttachment.target = null;
        catchTool.CatchBall = null;
        IsHandActive = false;
    }
}
