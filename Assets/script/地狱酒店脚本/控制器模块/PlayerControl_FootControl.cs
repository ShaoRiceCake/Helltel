using UnityEngine;

[RequireComponent(typeof(RaycastTool))]
[RequireComponent(typeof(SpringTool))]
public abstract class PlayerControl_FootControl : PlayerControl_BaseControl
{
    public GameObject targetObject;
    public GameObject forwardObject;
    public GameObject footObject;
    public float mouseSensitivity = 1f;
    public float impulseCoefficient = 0.3f; 

    protected Vector3 impulseDirection;
    protected bool isFootUp = false;
    protected bool isObjectFixed = false;
    protected RaycastTool raycastTool;
    protected SpringTool springTool;
    protected Rigidbody movingObj;
    protected float timeCounter_Move;
    protected float timeCounter_Hover;


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

        if (!CheckRequiredComponents())
        {
            return;
        }

        springTool.SetTarget(targetObject.transform);
        springTool.SetControlledObject(footObject);
        raycastTool.rayLauncher = footObject;

        SubscribeEvents();
    }

    protected bool CheckRequiredComponents()
    {
        string missingComponent =
            !footObject ? nameof(footObject) :
            !forwardObject ? nameof(forwardObject) :
            !springTool ? nameof(springTool) :
            !movingObj ? nameof(movingObj) :
            !raycastTool ? nameof(raycastTool) : null;

        if (missingComponent != null)
        {
            Debug.LogError($"Missing component: {missingComponent} on {gameObject.name}");
            return false;
        }

        return true;
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
        controlHandler.onMouseMove.AddListener(OnMouseMove);
    }

    protected virtual void UnsubscribeEvents()
    {
        controlHandler.onCancelLegGrab.RemoveListener(OnCancelLegGrab);
        controlHandler.onDefaultMode.RemoveListener(OnDefaultMode);
        controlHandler.onMouseMove.RemoveListener(OnMouseMove);
    }


    protected void OnCancelLegGrab()
    {

    }

    protected void OnDefaultMode()
    {

    }

    protected void OnMouseMove(Vector2 mouseDelta)
    {
        if (isFootUp)
        {
            float mouseX = mouseDelta.x * mouseSensitivity;
            float mouseY = mouseDelta.y * mouseSensitivity;

            Vector3 forwardDirection = forwardObject.transform.forward;
            forwardDirection.y = 0; 
            forwardDirection.Normalize();

            Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

            Vector3 impulseDirection = (rightDirection * mouseX + forwardDirection * mouseY).normalized;

            movingObj.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
        }
    }

    protected virtual void Update()
    {
        Vector3 hitPos = raycastTool.GetHitPoint();

        if (!isFootUp && hitPos != Vector3.zero)
        {
            MoveToTargetPosition(hitPos);
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

        if (distance > 0.1)
        {
            movingObj.velocity = direction.normalized * 10;
        }
        else
        {
            FixObject();
        }
    }


}