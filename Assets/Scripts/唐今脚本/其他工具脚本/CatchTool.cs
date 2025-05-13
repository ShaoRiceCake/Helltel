using UnityEngine;
using Obi;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class CatchTool : MonoBehaviour
{
    [Header("References")]
    public ObiParticleAttachment obiAttachment;
    public CatchDetectorTool catchDetectorTool;
    public ProgressBarPro progressBar; // 新增进度条引用

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
    private const float ShortPressThreshold = 0.2f; // 短按时间阈值
    private const float LongPressThreshold = 1f; // 长按时间阈值
    
    // 当前抓取的物品（只读）
    public ItemBase CurrentlyGrabbedItem { get; private set; }
    
    public GameObject CatchBall
    {
        get => _catchBall;
        set
        {
            if (_catchBall == value) return;

            if (_catchBall != null && value == null && _isGrabbing && _isGrabbingKinematic)
            {
                ReleaseObject();
            }

            if (_currentTarget && _catchBall != value)
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
        // 初始化进度条
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(false);
        }
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
        
            // 初始化进度条
            if (_isGrabbing && progressBar)
            {
                progressBar.gameObject.SetActive(true);
                progressBar.SetValue(0f); // 重置为0
            }
        }

        // 持续检测按键是否被按住足够长时间
        if (_isPressingE && Input.GetKey(KeyCode.E))
        {
            float pressDuration = Time.time - _pressTime;

            // 超过短按阈值但未到长按阈值时更新进度条
            if (pressDuration >= ShortPressThreshold && pressDuration < LongPressThreshold && _isGrabbing)
            {
                if (progressBar)
                {
                    // 计算0-1的进度比例 (从短按后开始计算)
                    float progressTime = pressDuration - ShortPressThreshold;
                    float totalProgressDuration = LongPressThreshold - ShortPressThreshold;
                    float progress = Mathf.Clamp01(progressTime / totalProgressDuration);
                    
                    progressBar.SetValue(progress);
                }
            }
            // 长按逻辑
            else if (pressDuration >= LongPressThreshold && _isGrabbing)
            {
                TryUseItem();
                _isPressingE = false;
                HideProgressBar();
                return;
            }
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            _isPressingE = false;
            HideProgressBar();
        
            // 短按逻辑
            if (Time.time - _pressTime < LongPressThreshold)
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
    }

    // 二次缓入函数 (开始慢，结束快)
    private float EaseInQuad(float t) {
        return t * t;
    }

    // 如果需要更明显的效果，可以使用三次缓入
    private float EaseInCubic(float t) {
        return t * t * t;
    }

    private void HideProgressBar()
    {
        if (progressBar)
        {
            progressBar.gameObject.SetActive(false);
        }
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
        
        if (!forceRelease && !item.RequestStateChange(EItemState.ReadyToGrab, CatchToolInstanceId, playerID))
            return false;
        
        if (CurrentlyGrabbedItem)
        {
            CurrentlyGrabbedItem.OnReleased.RemoveListener(OnItemReleased);
        }
            
        _isGrabbing = false;
        CurrentlyGrabbedItem = null;
        obiAttachment.enabled = false;
        obiAttachment.BindToTarget(null);
            
        AudioManager.Instance.Play("玩家松手", _catchBall.transform.position, 0.3f);
        return true;
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
        
        if (item.itemPrice > 0 && !item.IsPurchase)
        {
            var price = item.itemPrice;
            var hasEnoughMoney = CheckPlayerMoney(playerID, price);
        
            if (hasEnoughMoney)
            {
                DeductPlayerMoney(playerID, price);
                item.IsPurchase = true;
            
                AudioManager.Instance.Play("购买", _catchBall.transform.position, 0.7f);
            }
            else
            {
                AudioManager.Instance.Play("无法购买", _catchBall.transform.position, 0.7f);
                return;
            }
        }
        
        if (!item.RequestStateChange(EItemState.Grabbed, CatchToolInstanceId, playerID)) return;

        _isGrabbing = true;
        CurrentlyGrabbedItem = item;

        item.OnReleased.AddListener(OnItemReleased);

        var rb = item.GetComponent<Rigidbody>();
        _isGrabbingKinematic = rb && rb.isKinematic;

        obiAttachment.BindToTarget(item.transform);
        obiAttachment.enabled = true;
        AudioManager.Instance.Play("玩家抓取", _catchBall.transform.position, 0.7f);
    }

    private void OnItemReleased()
    {
        if (!_isGrabbing) return;
        if (CurrentlyGrabbedItem)
        {
            CurrentlyGrabbedItem.OnReleased.RemoveListener(OnItemReleased);
        }
        
        _isGrabbing = false;
        _isGrabbingKinematic = false;
        CurrentlyGrabbedItem = null;
        obiAttachment.enabled = false;
        obiAttachment.BindToTarget(null);
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
        if (!rb) return 0f;
    
        return rb.isKinematic ? float.MaxValue : 
            rb.mass;
    }
    
    public bool IsGrabbingKinematic()
    {
        return _isGrabbing && _isGrabbingKinematic;
    }
    
    private bool CheckPlayerMoney(ulong playerId, int price)
    {
        return GameController.Instance.GetMoney() >= price;
    }

    private void DeductPlayerMoney(ulong playerId, int amount)
    {
        GameController.Instance.DeductMoney(amount);
    }
}