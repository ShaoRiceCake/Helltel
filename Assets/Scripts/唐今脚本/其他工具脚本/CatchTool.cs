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
    public ulong playerID = 0;

    private bool _isGrabbingKinematic = false;
    private float _pressTime;
    private bool _isPressingE;
    private const float LongPressThreshold = 0.5f; // 长按时间检测阈值
    
    // 当前抓取的物品（只读）
    public ItemBase CurrentlyGrabbedItem { get; private set; }
    
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
        if (Input.GetKeyDown(KeyCode.E))
        {
            _pressTime = Time.time;
            _isPressingE = true;
        }

        // 持续检测按键是否被按住足够长时间
        if (_isPressingE && Input.GetKey(KeyCode.E))
        {
            var pressDuration = Time.time - _pressTime;
            if (pressDuration >= LongPressThreshold && _isGrabbing)
            {
                TryUseItem();
                _isPressingE = false; // 重置状态
                return;
            }
        }

        if (!Input.GetKeyUp(KeyCode.E)) return;
    
        if (_isPressingE)
        {
            var pressDuration = Time.time - _pressTime;
        
            // 短按逻辑
            if (pressDuration < LongPressThreshold)
            {
                if (_currentTarget && !_isGrabbing)
                {
                    GrabObject(_currentTarget);
                }
                else if (_isGrabbing)
                {
                    ReleaseObject();
                }
            }
        }
        _isPressingE = false;
    }

    /// <summary>
    /// 外部调用强制释放当前抓取的物体
    /// </summary>
    /// <param name="forceRelease">是否强制释放（忽略权限检查）</param>
    /// <returns>是否成功释放</returns>
    public bool ForceRelease(bool forceRelease = false)
    {
        if (!_isGrabbing) return false;
        
        if (!obiAttachment.target || 
            !obiAttachment.target.gameObject.TryGetComponent<ItemBase>(out var item))
        {
            return false;
        }
        
        // 如果是强制释放或者有权限释放
        if (forceRelease || item.RequestStateChange(EItemState.ReadyToGrab, CatchToolInstanceId, playerID))
        {
            _isGrabbing = false;
            CurrentlyGrabbedItem = null;
            obiAttachment.enabled = false;
            obiAttachment.BindToTarget(null);
            
            AudioManager.Instance.Play("玩家松手", _catchBall.transform.position, 0.3f);
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// 检查当前是否正在抓取物体
    /// </summary>
    public bool IsGrabbing()
    {
        return _isGrabbing;
    }

    /// <summary>
    /// 获取当前抓取的物品（如果没有则返回null）
    /// </summary>
    public ItemBase GetGrabbedItem()
    {
        return CurrentlyGrabbedItem;
    }

    private void TryUseItem()
    {
        if (!_isGrabbing || !obiAttachment.target) return;
    
        var item = obiAttachment.target.gameObject.GetComponent<ItemBase>();
        if (!item) return;
    
        if (item is IUsable usableItem)
        {
            usableItem.OnUseStart?.Invoke();
        }
        else
        {
            Debug.Log($"[CatchTool] 这个道具({item.ItemName})不能被使用");
        }
    }


    private void GrabObject(GameObject target)
    {
        if (!target || !target.TryGetComponent<ItemBase>(out var item)) return;
        if (!item.RequestStateChange(EItemState.Grabbed, CatchToolInstanceId, playerID)) return;

        _isGrabbing = true;
        CurrentlyGrabbedItem = item;
    
        // 检查是否为运动学对象
        var rb = item.GetComponent<Rigidbody>();
        _isGrabbingKinematic = rb && rb.isKinematic;
    
        obiAttachment.BindToTarget(item.transform);
        obiAttachment.enabled = true;
        AudioManager.Instance.Play("玩家抓取", _catchBall.transform.position, 0.7f);
    }
    
    private void ReleaseObject()
    {
        _isGrabbingKinematic = false; 
        ForceRelease();
    }
    
    public float GetGrabbedItemMass()
    {
        if (!_isGrabbing || !CurrentlyGrabbedItem) return 0f;
    
        var rb = CurrentlyGrabbedItem.GetComponent<Rigidbody>();
        if (rb == null) return 0f;
    
        if (rb.isKinematic) return float.MaxValue; // Treat kinematic objects as infinite mass
        return rb.mass;
    }
    
    public bool IsGrabbingKinematic()
    {
        return _isGrabbing && _isGrabbingKinematic;
    }
}