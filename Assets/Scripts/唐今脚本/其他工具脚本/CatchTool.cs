using UnityEngine;
using Obi;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class CatchTool : MonoBehaviour
{
    [Header("References")]
    public ObiParticleAttachment obiAttachment;
    public CatchDetectorTool catchDetectorTool; 

    private SphereCollider _sphereCollider;
    private GameObject _catchBall;
    private bool _isGrabbing;
    private GameObject _currentTarget;
    private Transform _catchAimTrans; 
    private GameObject _currentHighlightedObject;
    private OutlineController _currentGrabbedOutline; 

    // 属性封装
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
        if (catchDetectorTool == null)
        {
            Debug.LogError("catchDetectCylinder未分配！");
            return;
        }
        
        catchDetectorTool.OnDetectedObjectsUpdated += UpdatePreSelectedObjects;
    }

    private void Update()
    {
        if(!_catchBall) return;
        HighLightTarget();
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

        if (preSelectedObjects.Count > 0)
        {
            _currentTarget = preSelectedObjects
                .OrderBy(obj => Vector3.Distance(
                    obj.transform.position, 
                    _catchAimTrans.position))
                .FirstOrDefault();
        }
        else
        {
            _currentTarget = null;
        }
    }

    private void HighLightTarget()
    {
        // 如果正在抓取或者新目标就是当前已高亮的物体，则不做任何操作
        if (_isGrabbing || _currentTarget == _currentHighlightedObject) return;
    
        // 先取消旧物体的高亮
        if (_currentHighlightedObject)
        {
            var oldOutline = _currentHighlightedObject.GetComponent<OutlineController>();
            if (oldOutline)
                oldOutline.SetOutlineEnabled(false);
        }
    
        // 高亮新物体
        if (_currentTarget)
        {
            var newOutline = _currentTarget.GetComponent<OutlineController>();
            if (!newOutline) return;
            
            newOutline.SetOutlineEnabled(true);
            _currentHighlightedObject = _currentTarget;
        }
        else
        {
            _currentHighlightedObject = null;
        }
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
        if(!target) return;
        
        _currentGrabbedOutline = target.GetComponent<OutlineController>();
        if (_currentGrabbedOutline)
        {
            _currentGrabbedOutline.SetOutlineEnabled(false); 
            _currentGrabbedOutline.LockOutlineState();
        }
        
        _isGrabbing = true;
        obiAttachment.BindToTarget(target.transform);
        obiAttachment.enabled = true;
        AudioManager.Instance.Play("玩家抓取",_catchBall.transform.position,0.7f);

    }

    private void ReleaseObject()
    {
        if (!_isGrabbing) return;
        
        if (_currentGrabbedOutline)
        {
            _currentGrabbedOutline.UnlockOutlineState();
            _currentGrabbedOutline = null;
        }
        
        _isGrabbing = false;
        obiAttachment.enabled = false;
        obiAttachment.BindToTarget(null);
        AudioManager.Instance.Play("玩家松手",_catchBall.transform.position,0.3f);
    }
}