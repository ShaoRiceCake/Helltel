using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(RaycastTool))]
[RequireComponent(typeof(SpringTool))]
public abstract class PlayerControl_FootControl : PlayerControl_BaseControl
{
    public GameObject targetObject;
    public GameObject footObject;

    public float mouseSensitivity = 1f;
    public float impulseCoefficient = 0.4f;
        
    public RaycastTool raycastTool;
    public SpringTool springTool;
    
    protected enum FootState
    {
        Grounded, 
        Lifted,   
        Locked     
    }
    
    protected FootState CurrentState = FootState.Grounded;

    private Rigidbody _movingObj;
    
    protected override void Start()
    {
        base.Start();

        if (!footObject)
        {
            if (gameObject != null) footObject = gameObject;
        }
        _movingObj = footObject.GetComponent<Rigidbody>();

        NullCheckerTool.CheckNull(raycastTool, springTool, _movingObj);

        springTool.SetTarget(targetObject.transform);
        springTool.SetControlledObject(footObject);
        raycastTool.rayLauncher = footObject;

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
        controlHandler.onMouseMoveFixedUpdate.AddListener(OnMouseMove);
    }

    protected virtual void UnsubscribeEvents()
    {
        controlHandler.onMouseMoveFixedUpdate.RemoveListener(OnMouseMove);
    }

    private void OnMouseMove(Vector2 mouseDelta)
    {
        if (CurrentState != FootState.Lifted) return;
        
        var mouseX = mouseDelta.x * mouseSensitivity;
        var mouseY = mouseDelta.y * mouseSensitivity;

        var forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; 
        forwardDirection.Normalize();

        var rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;
        var horizontalImpulseDirection = (rightDirection * mouseX + forwardDirection * mouseY).normalized;
        var impulseDirection = (horizontalImpulseDirection + Vector3.down/2).normalized;

        _movingObj.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
    }
    
    protected virtual void Update()
    {
        var hitPos = raycastTool.GetHitPoint();

        if (CurrentState == FootState.Grounded && hitPos != Vector3.zero)
        {
            MoveToTargetPosition(hitPos);
        }
    }

    private void UnfixObject()
    {
        _movingObj.isKinematic = false;
    }

    private void FixObject()
    {
        _movingObj.velocity = Vector3.zero;
        _movingObj.angularVelocity = Vector3.zero;
        _movingObj.isKinematic = true;
    }

    private void MoveToTargetPosition(Vector3 targetPosition)
    {
        var direction = targetPosition - _movingObj.position;
        var distance = direction.magnitude;

        if (distance > 0.2)
        {
            _movingObj.velocity = direction.normalized * 15;
        }
        else
        {
            FixObject();
        }
    }
    
    protected bool TryLiftFoot()
    {
        if (CurrentState == FootState.Locked) return false;
        
        CurrentState = FootState.Lifted;
        UnfixObject();
        springTool.isSpringEnabled = true;
        return true;
    }
    
    protected void ReleaseFoot()
    {
        if (CurrentState != FootState.Lifted) return;
        
        CurrentState = FootState.Grounded;
        springTool.isSpringEnabled = false;
    }
    
    protected void LockFoot()
    {
        if (CurrentState == FootState.Lifted)
        {
            ReleaseFoot(); 
        }
        CurrentState = FootState.Locked;
    }
    
    protected void UnlockFoot()
    {
        if (CurrentState == FootState.Locked)
        {
            CurrentState = FootState.Grounded;
        }
    }
}