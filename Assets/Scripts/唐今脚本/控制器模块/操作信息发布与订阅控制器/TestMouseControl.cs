using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
public class TestMouseControl : MonoBehaviour
{
    public float MouseSensitivity
    {
        get => _mMouseSensitivity;
        set
        {
            if (Mathf.Approximately(value, _mMouseSensitivity)) return;
            _mMouseSensitivity = value;
            if (_mMouseSensitivity == 0)
            {
                Debug.LogWarning("mouseSensitivity is zero!");
            }
        }
    }
    
    private bool EnableMouseControl { get; set; } = true;

    private float _mMouseSensitivity = 1;

    // ÊÂ¼þ¶¨Òå
    public UnityEvent onLeftMouseDown;        
    public UnityEvent onRightMouseDown;       
    public UnityEvent onLeftMouseUp;          
    public UnityEvent onRightMouseUp;       
    public UnityEvent onMiddleMouseDown;      
    public UnityEvent onMouseWheelUp;          
    public UnityEvent onMouseWheelDown;      
    public UnityEvent onBothMouseButtonsDown; 
    public UnityEvent<Vector2> onMouseMoveFixedUpdate; 
    public UnityEvent<Vector2> onMouseMoveUpdate;    
    public UnityEvent onNoMouseButtonDown; 

    private Vector2 _lastMousePosition;

    private void Awake()
    {
        onLeftMouseDown ??= new UnityEvent();
        onRightMouseDown ??= new UnityEvent();
        onLeftMouseUp ??= new UnityEvent();
        onRightMouseUp ??= new UnityEvent();
        onMiddleMouseDown ??= new UnityEvent();
        onMouseWheelUp ??= new UnityEvent();
        onMouseWheelDown ??= new UnityEvent();
        onBothMouseButtonsDown ??= new UnityEvent();
        onNoMouseButtonDown ??= new UnityEvent();
        onMouseMoveFixedUpdate ??= new UnityEvent<Vector2>();
        onMouseMoveUpdate ??= new UnityEvent<Vector2>();
    }

    private void Update()
    {
        HandleMouseButtons();
        HandleMouseWheel();
        HandleMouseMovementUpdate(); 

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            onNoMouseButtonDown?.Invoke();
        }
    }

    private void FixedUpdate()
    {
        if (!EnableMouseControl)
            return;

        HandleMouseMovementFixedUpdate(); 
    }

    private void HandleMouseButtons()
    {
        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            onLeftMouseDown?.Invoke();
        }

        if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0))
        {
            onRightMouseDown?.Invoke();
        }

        if (Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1))
        {
            onLeftMouseUp?.Invoke();
        }

        if (Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0))
        {
            onRightMouseUp?.Invoke();
        }

        if (Input.GetMouseButtonDown(2))
        {
            onMiddleMouseDown?.Invoke();
        }

        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            onBothMouseButtonsDown?.Invoke();
        }
    }

    private void HandleMouseWheel()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");

        switch (scroll)
        {
            case > 0:
                onMouseWheelUp?.Invoke();
                break;
            case < 0:
                onMouseWheelDown?.Invoke();
                break;
        }
    }

    private void HandleMouseMovementFixedUpdate()
    {
        var currentMousePosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (currentMousePosition != _lastMousePosition)
        {
            var mouseDelta = currentMousePosition * (_mMouseSensitivity * Time.fixedDeltaTime);
            onMouseMoveFixedUpdate?.Invoke(mouseDelta);
        }
        _lastMousePosition = currentMousePosition;
    }

    private void HandleMouseMovementUpdate()
    {
        var currentMousePosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (currentMousePosition != _lastMousePosition)
        {
            var mouseDelta = currentMousePosition * _mMouseSensitivity;
            onMouseMoveUpdate?.Invoke(mouseDelta);
        }
        _lastMousePosition = currentMousePosition;
    }
}