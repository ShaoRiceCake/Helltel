using UnityEngine;

public abstract class PlayerController_Base : MonoBehaviour
{
    [Header("¿ØÖÆ²ÎÊý")]
    public float mouseSensitivity = 0.01f;
    public float limit = 3f;

    protected Vector3 currentMousePos;
    protected Vector3 lastMousePos;
    protected Vector3 mouseDelta;
    protected GameObject targetObject;
    protected bool isActive;

    protected virtual void Start()
    {
        lastMousePos = Input.mousePosition;
        InitializeComponents();
    }

    protected virtual void Update()
    {
        if (!isActive) return;

        UpdateMouseDelta();
        HandleInput();
        ApplyMovement();
    }

    protected void UpdateMouseDelta()
    {
        currentMousePos = Input.mousePosition;
        mouseDelta = currentMousePos - lastMousePos;
        lastMousePos = currentMousePos;
    }

    protected abstract void InitializeComponents();
    protected abstract void HandleInput();
    protected abstract void ApplyMovement();

    public virtual void SetActive(bool active)
    {
        isActive = active;
        if (!active) ResetPosition();
    }

    protected virtual void ResetPosition()
    {
        if (targetObject != null)
        {
            targetObject.transform.position = transform.position;
        }
    }
}
