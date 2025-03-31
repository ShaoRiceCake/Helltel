using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RaycastTool))]
[RequireComponent(typeof(SpringTool))]
[RequireComponent(typeof(Rigidbody))]
public abstract class PlayerControl_FootControl : PlayerControl_BaseControl
{
    public GameObject targetObject;
    public GameObject footObject;

    public float mouseSensitivity = 1f;
    public float impulseCoefficient = 0.3f; 
    
    protected bool IsFootUp = false;
    private bool _isObjectFixed ;
    private bool _isCanceling;

    private RaycastTool _raycastTool;
    protected SpringTool SpringTool;
    private Rigidbody _movingObj;
    
    private float _timeCounterCancel;

    protected override void Start()
    {
        base.Start();

        if (!footObject)
        {
            if (gameObject != null) footObject = gameObject;
        }

        _raycastTool = footObject.GetComponent<RaycastTool>();
        SpringTool = footObject.GetComponent<SpringTool>();
        _movingObj = footObject.GetComponent<Rigidbody>();

        NullCheckerTool.CheckNull(_raycastTool, SpringTool, _movingObj);

        SpringTool.SetTarget(targetObject.transform);
        SpringTool.SetControlledObject(footObject);
        _raycastTool.rayLauncher = footObject;

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
        controlHandler.onCancelLegGrab.AddListener(OnCancelLegGrab);
        controlHandler.onDefaultMode.AddListener(OnDefaultMode);
        controlHandler.onMouseMoveFixedUpdate.AddListener(OnMouseMove);
    }

    protected virtual void UnsubscribeEvents()
    {
        controlHandler.onCancelLegGrab.RemoveListener(OnCancelLegGrab);
        controlHandler.onDefaultMode.RemoveListener(OnDefaultMode);
        controlHandler.onMouseMoveFixedUpdate.RemoveListener(OnMouseMove);
    }


    private void OnCancelLegGrab()
    {
    }

    private void OnDefaultMode()
    {

    }

    private void OnMouseMove(Vector2 mouseDelta)
    {
        if (!IsFootUp) return;
        var mouseX = mouseDelta.x * mouseSensitivity;
        var mouseY = mouseDelta.y * mouseSensitivity;

        var forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; 
        forwardDirection.Normalize();

        var rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // 计算水平方向上的冲量方向
        var horizontalImpulseDirection = (rightDirection * mouseX + forwardDirection * mouseY).normalized;

        // 将冲量方向改为斜下方45度方向
        var impulseDirection = (horizontalImpulseDirection + Vector3.down/2).normalized;

        _movingObj.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
    }
    
    protected virtual void Update()
    {
        var hitPos = _raycastTool.GetHitPoint();

        if (!IsFootUp && hitPos != Vector3.zero)
        {
            MoveToTargetPosition(hitPos);
        }
    }

    protected void UnfixObject()
    {
        _movingObj.isKinematic = false;
        _isObjectFixed = false;
    }

    private void FixObject()
    {
        if (_isObjectFixed) return;
        _movingObj.velocity = Vector3.zero;
        _movingObj.angularVelocity = Vector3.zero;
        _movingObj.isKinematic = true;
        _isObjectFixed = true;
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
}