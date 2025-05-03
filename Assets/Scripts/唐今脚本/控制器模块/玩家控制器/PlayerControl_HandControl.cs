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
    
    [Header("Weight Sensitivity")]
    public float baseMouseSensitivity = 10f; // Rename the existing mouseSensitivity to baseMouseSensitivity
    public float maxMassForFullSensitivity = 5f; // Objects below this mass won't affect sensitivity
    public float minSensitivityMultiplier = 0.1f; // Minimum sensitivity when holding very heavy objects

    private float _currentMouseSensitivity;
    protected int CurrentPlayerHand;
    private bool _isMouseDown;
    protected bool IsHandActive;

    private Vector3 _targetPosition;
    [SerializeField] private float smoothSpeed = 5f;
    private Vector3 _smoothDampVelocity;

    public int CurrentHand
    {
        get => CurrentPlayerHand;
        private set
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
        
        _currentMouseSensitivity = baseMouseSensitivity;
        mouseSensitivity = baseMouseSensitivity; // Initialize
        
        SubscribeEvents();
    }


    public void OnDestroy()
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
        controlHandler.onMouseMoveFixedUpdate.AddListener(OnMouseMove);

    }

    protected virtual void UnsubscribeEvents()
    {
        controlHandler.onCancelHandGrab.RemoveListener(OnCancelHandGrab);
        controlHandler.onMouseMoveFixedUpdate.RemoveListener(OnMouseMove);

    }

    protected virtual void Update()
    {
        if(controlHandler.mCurrentControlMode == ControlMode.LegControl)
        {
            CurrentHand = 0;
        }
        
        UpdateSensitivityBasedOnMass(catchTool);
        HandleKinematicObjectGrabbing();
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
        if (catchTool != null && catchTool.IsGrabbingKinematic())
            return;

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
        
        _targetPosition = handBallPrefab.transform.position + worldMovement * Time.deltaTime;

        var newLocalPosition = handPrepareObj.transform.InverseTransformPoint(_targetPosition);

        ApplyCylinderConstraint(ref newLocalPosition);

        _targetPosition = handPrepareObj.transform.TransformPoint(newLocalPosition);
        
        handBallPrefab.transform.position = Vector3.SmoothDamp(
            handBallPrefab.transform.position,
            _targetPosition,
            ref _smoothDampVelocity,
            1/smoothSpeed 
        );
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
    
        if (catchTool && catchTool.IsGrabbingKinematic() && catchTool.GetGrabbedItem() != null)
        {
            handBallPrefab.transform.position = catchTool.GetGrabbedItem().transform.position;
        }
        else
        {
            handBallPrefab.transform.position = ObiGetGroupParticles.GetParticleWorldPositions(handControlAttachment)[0];
        }
    
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
    
    protected void UpdateSensitivityBasedOnMass(CatchTool catchTool)
    {
        if (!catchTool || !catchTool.IsGrabbing())
        {
            // No item grabbed or no catch tool - use full sensitivity
            mouseSensitivity = baseMouseSensitivity;
            return;
        }

        var mass = catchTool.GetGrabbedItemMass();
    
        if (mass >= float.MaxValue)
        {
            // Infinite mass or kinematic - can't move
            mouseSensitivity = 0f;
        }
        else if (mass <= maxMassForFullSensitivity)
        {
            // Light object - full sensitivity
            mouseSensitivity = baseMouseSensitivity;
        }
        else
        {
            // Calculate sensitivity based on mass
            var massEffect = Mathf.Clamp01((mass - maxMassForFullSensitivity) / (maxMassForFullSensitivity * 10f));
            mouseSensitivity = Mathf.Lerp(baseMouseSensitivity, baseMouseSensitivity * minSensitivityMultiplier, massEffect);
        }
    
        _currentMouseSensitivity = mouseSensitivity;
    }
    
    private void HandleKinematicObjectGrabbing()
    {
        if (!catchTool || !catchTool.IsGrabbingKinematic() || !handBallPrefab)
            return;
    
        if (catchTool.GetGrabbedItem())
        {
            handBallPrefab.transform.position = catchTool.GetGrabbedItem().transform.position;
        }
    }
}


