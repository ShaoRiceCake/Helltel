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

    protected Vector3 impulseDirection;

    protected bool isFootUp = false;
    protected bool isObjectFixed = false;
    protected bool isCancleing = false;

    protected RaycastTool raycastTool;
    protected SpringTool springTool;
    protected Rigidbody movingObj;

    protected float timeCounter_Move = 0f;
    protected float timeCounter_Hover = 0f;
    protected float timeCounter_Cancle = 0f;

    protected override void Start()
    {
        base.Start();

        if (!footObject)
        {
            footObject = this.gameObject;
        }

        raycastTool = footObject.GetComponent<RaycastTool>();
        springTool = footObject.GetComponent<SpringTool>();
        movingObj = footObject.GetComponent<Rigidbody>();

        NullCheckerTool.CheckNull(raycastTool, springTool, movingObj);

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

    protected virtual void SubscribeEvents()
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


    protected void OnCancelLegGrab()
    {
        isCancleing = true;
        timeCounter_Cancle = 0f;
    }

    protected void OnDefaultMode()
    {

    }

    protected IEnumerator HandleCancelLegGrab()
    {

        if (isCancleing)
        {
            DisableInteractions();
            yield return new WaitForSeconds(1f);
            EnableInteractions();
        }
    }

    private void DisableInteractions()
    {

    }

    private void EnableInteractions()
    {

    }

    // protected void OnMouseMove(Vector2 mouseDelta)
    // {
    //     if (isFootUp)
    //     {
    //         float mouseX = mouseDelta.x * mouseSensitivity;
    //         float mouseY = mouseDelta.y * mouseSensitivity;
    //
    //         Vector3 forwardDirection = forwardObject.transform.forward;
    //         forwardDirection.y = 0; 
    //         forwardDirection.Normalize();
    //
    //         Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;
    //
    //         Vector3 impulseDirection = (rightDirection * mouseX + forwardDirection * mouseY).normalized;
    //
    //         movingObj.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
    //     }
    // }

    private void OnMouseMove(Vector2 mouseDelta)
    {
        if (!isFootUp) return;
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

        movingObj.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
    }
    
    protected virtual void Update()
    {
        Vector3 hitPos = raycastTool.GetHitPoint();

        if (!isFootUp && hitPos != Vector3.zero)
        {
            MoveToTargetPosition(hitPos);
        }

        if (isCancleing)
        {
            timeCounter_Cancle += Time.deltaTime;

            if (timeCounter_Cancle >= 1f)
            {
                HandleCancelLegGrab();
                timeCounter_Cancle = 0f; 
                isCancleing = false;
            }
        }
    }

    protected void UnfixObject()
    {
        movingObj.isKinematic = false;
        isObjectFixed = false;
    }
    protected void FixObject()
    {
        if (!isObjectFixed)
        {
            movingObj.velocity = Vector3.zero;
            movingObj.angularVelocity = Vector3.zero;
            movingObj.isKinematic = true;
            isObjectFixed = true;
        }
    }

    protected void MoveToTargetPosition(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - movingObj.position;
        float distance = direction.magnitude;

        if (distance > 0.2)
        {
            movingObj.velocity = direction.normalized * 15;
        }
        else
        {
            FixObject();
        }
    }


}