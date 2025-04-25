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
    private int CatchToolInstanceId => GetInstanceID();
    public ulong playerID;

    public GameObject CatchBall
    {
        get => _catchBall;
        set
        {
            if (_catchBall != value && _currentTarget)
            {
                if (_currentTarget.TryGetComponent<ItemBase>(out var item))
                {
                    item.RequestStateChange(EItemState.Selected);
                }
                _currentTarget = null;
            }

            _catchBall = value;
            if (!_catchBall) return;
        
            _sphereCollider = _catchBall.GetComponent<SphereCollider>();
            _catchAimTrans = _catchBall.transform;
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
    }

    private void UpdatePreSelectedObjects(List<GameObject> detectedObjects)
    {
        foreach (var obj in preSelectedObjects.Except(detectedObjects))
        {
            if (obj && obj.TryGetComponent<ItemBase>(out var item) && 
                !item.IsGrabbed) 
            {
                item.RequestStateChange(EItemState.NotSelected);
            }
        }

        foreach (var obj in detectedObjects)
        {
            if (obj && obj.TryGetComponent<ItemBase>(out var item) && 
                item.IsNotSelected) 
            {
                item.RequestStateChange(EItemState.Selected);
            }
        }

        preSelectedObjects = detectedObjects;
    }
    
    private void UpdateTargetSelection()
    {
        if (!_sphereCollider) return;
        
        GameObject newTarget = null;
        
        if (preSelectedObjects.Count > 0)
        {
            newTarget = preSelectedObjects
                .OrderBy(obj => Vector3.Distance(
                    obj.transform.position, 
                    _catchAimTrans.position))
                .FirstOrDefault();
        }

        if (_currentTarget == newTarget) return;
        if (_currentTarget && _currentTarget.TryGetComponent<ItemBase>(out var prevItem))
        {
            prevItem.RequestStateChange(EItemState.Selected);
        }

        if (newTarget && newTarget.TryGetComponent<ItemBase>(out var newItem))
        {
            newItem.RequestStateChange(EItemState.ReadyToGrab);
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
        if(!target) return;

        if (!target.TryGetComponent<ItemBase>(out var item)) return;
        if (!item.RequestStateChange(EItemState.Grabbed, CatchToolInstanceId, playerID)) return;
        _isGrabbing = true;
        obiAttachment.BindToTarget(target.transform);
        obiAttachment.enabled = true;
                
        AudioManager.Instance.Play("玩家抓取", _catchBall.transform.position, 0.7f);
    }

    private void ReleaseObject()
    {
        if (!_isGrabbing) return;

        if (!obiAttachment.target ||
            !obiAttachment.target.gameObject.TryGetComponent<ItemBase>(out var item)) return;
        if (!item.RequestStateChange(EItemState.ReadyToGrab, CatchToolInstanceId, playerID)) return;
        _isGrabbing = false;
        obiAttachment.enabled = false;
        obiAttachment.BindToTarget(null);
                
        AudioManager.Instance.Play("玩家松手", _catchBall.transform.position, 0.3f);
    }
}