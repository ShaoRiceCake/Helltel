using UnityEngine;
using Obi;
using System.Collections.Generic;
using System.Linq;

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
    private ItemBase _currentItem;

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
        UpdateTargetSelection();
        HandleInput();
        UpdateItemStates();
    }

    private void UpdatePreSelectedObjects(List<GameObject> detectedObjects)
    {
        foreach (var item in preSelectedObjects.Select(obj => obj.GetComponent<ItemBase>()).Where(item => item))
        {
            item.UpdateGraspingState(EGraspingState.NotSelected);
        }

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

            if (!_currentTarget) return;
            _currentItem = _currentTarget.GetComponent<ItemBase>();
            if (_currentItem && !_isGrabbing)
            {
                _currentItem.UpdateGraspingState(EGraspingState.OnSelected);
            }
        }
        else
        {
            _currentTarget = null;
            _currentItem = null;
        }
    }
    private void UpdateItemStates()
    {
        if (_isGrabbing && _currentItem)
        {
            _currentItem.UpdateGraspingState(EGraspingState.OnCaught);
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
        
        _isGrabbing = true;
        obiAttachment.BindToTarget(target.transform);
        obiAttachment.enabled = true;
        
        _currentItem = target.GetComponent<ItemBase>();
        if (_currentItem)
        {
            _currentItem.UpdateGraspingState(EGraspingState.OnCaught);
            _currentItem.OnGrabbed?.Invoke();
        }
        
        AudioManager.Instance.Play("玩家抓取",_catchBall.transform.position,0.7f);
    }

    private void ReleaseObject()
    {
        if (!_isGrabbing) return;
        
        _isGrabbing = false;
        obiAttachment.enabled = false;
        obiAttachment.BindToTarget(null);
        
        if (_currentItem)
        {
            _currentItem.UpdateGraspingState(EGraspingState.NotSelected);
            _currentItem.OnReleased?.Invoke();
            _currentItem = null;
        }
        
        AudioManager.Instance.Play("玩家松手",_catchBall.transform.position,0.3f);
    }
}