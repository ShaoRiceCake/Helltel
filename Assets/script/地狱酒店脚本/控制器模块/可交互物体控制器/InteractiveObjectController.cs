using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OutlineController))]
public class InteractiveObjectController : MonoBehaviour
{
    private Dictionary<InteractiveState, IInteractiveState> _states;
    private IInteractiveState _currentState;
    private OutlineController _outlineController;
    private bool _isInitialized;

    private void Awake()
    {
        Initialize();
        ChangeState(InteractiveState.Normal);
    }

    private void Initialize()
    {
        if (_isInitialized) return;
        
        _outlineController = GetComponent<OutlineController>();
        if (!_outlineController)
        {
            Debug.LogError("OutlineController component missing on " + gameObject.name, this);
            return;
        }
        
        // 初始化状态机
        _states = new Dictionary<InteractiveState, IInteractiveState>
        {
            { InteractiveState.Normal, new NormalState() },
            { InteractiveState.Selected, new SelectedState() },
            { InteractiveState.Grabbed, new GrabbedState() }
        };
        
        _isInitialized = true;
    }

    public void ChangeState(InteractiveState newState)
    {
        if (!_isInitialized) Initialize();
        if (!_isInitialized) return;
        
        _currentState?.ExitState(this);
        _currentState = _states[newState];
        _currentState.EnterState(this);
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (!_isInitialized) Initialize();
        if (!_isInitialized) return;
        
        _outlineController.SetOutlineEnabled(isHighlighted);
    }
    
    public void LockOutline()
    {
        _outlineController?.LockOutline();
    }

    public void UnlockOutline()
    {
        _outlineController?.UnlockOutline();
    }
}