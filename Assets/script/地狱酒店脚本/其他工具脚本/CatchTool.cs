using UnityEngine;
using Obi;
using System.Collections.Generic;
using System.Linq;

public class CatchTool : MonoBehaviour
{
    [Header("References")]
    public ObiParticleAttachment obiAttachment;
    public GameObject catchDetectCylinder; 

    private SphereCollider _sphereCollider;
    private GameObject _catchBall;
    private bool _isGrabbing;
    private GameObject _currentTarget;
    private Transform _catchAimTrans; 
    private GameObject _currentHighlightedObject;

    public GameObject CatchBall
    {
        get => _catchBall;
        set
        {
            _catchBall = value;
            if (!_catchBall) return;
            _sphereCollider = _catchBall.GetComponent<SphereCollider>();
            _catchAimTrans =  _catchBall.transform;
            NullCheckerTool.CheckNull(_catchBall, _sphereCollider, obiAttachment,_catchAimTrans);
        }
    }

    public List<GameObject> preSelectedObjects = new();

    private void Start()
    {
        if (catchDetectCylinder == null)
        {
            Debug.LogError("catchDetectCylinder未分配！");
            return;
        }

        var detector = catchDetectCylinder.GetComponent<CatchDetectorTool>();
        if (detector == null)
        {
            Debug.LogError("catchDetectCylinder上缺少CatchDetectorTool组件！");
            return;
        }
    
        detector.OnDetectedObjectsUpdated += UpdatePreSelectedObjects;
    }

    private void Update()
    {
        if(!_catchBall) return;
        
        UpdateTargetSelection();
        HandleInput();
    }

    private void UpdatePreSelectedObjects(List<GameObject> detectedObjects)
    {
        preSelectedObjects = detectedObjects;
    }

    private void UpdateTargetSelection()
    {
        if (!_sphereCollider) return;

        var newTarget = preSelectedObjects.Count > 0
            ? preSelectedObjects
                .OrderBy(obj => Vector3.Distance(obj.transform.position, _catchAimTrans.position))
                .FirstOrDefault()
            : null;
        
        if (_currentTarget && _currentTarget != newTarget)
        {
            var interactiveObj = _currentTarget.GetComponent<InteractiveObjectController>();
            if (interactiveObj)
                interactiveObj.ChangeState(InteractiveState.Normal);
        }

        if (newTarget && newTarget != _currentTarget)
        {
            var interactiveObj = newTarget.GetComponent<InteractiveObjectController>();
            if (interactiveObj)
                interactiveObj.ChangeState(InteractiveState.Selected);
        }

        _currentTarget = newTarget;
    }
    
    private void HandleInput()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        
        if (_currentTarget && !_isGrabbing)
        {
            GrabObject(_currentTarget);
        }
        else
        {
            ReleaseObject();
        }
    }

    private void GrabObject(GameObject target)
    {
        if (!target) return;
        
        var interactiveObj = target.GetComponent<InteractiveObjectController>();
        if (interactiveObj)
            interactiveObj.ChangeState(InteractiveState.Grabbed);
        
        _isGrabbing = true;
        obiAttachment.BindToTarget(target.transform);
        obiAttachment.enabled = true;
    }

    private void ReleaseObject()
    {
        if (!_isGrabbing) return;
        
        if (_currentTarget)
        {
            var interactiveObj = _currentTarget.GetComponent<InteractiveObjectController>();
            if (interactiveObj)
                interactiveObj.ChangeState(InteractiveState.Normal);
        }
        
        _isGrabbing = false;
        obiAttachment.enabled = false;
        obiAttachment.BindToTarget(null);
    }
}