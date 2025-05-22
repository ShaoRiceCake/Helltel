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

            if (_catchBall && !value && _isGrabbing && _isGrabbingKinematic)
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
        if (!catchDetectorTool)
        {
            Debug.LogError("catchDetectCylinder未分配！");
            return;
        }
        catchDetectorTool.OnDetectedObjectsUpdated += UpdatePreSelectedObjects;
        // 初始化进度条
        if (progressBar)
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
        
            // 初始化进度条（但不立即显示）
            if (_isGrabbing && progressBar)
            {
                progressBar.gameObject.SetActive(false); // 初始隐藏
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
                    // 只在第一次超过阈值时显示进度条
                    if (!progressBar.gameObject.activeSelf)
                    {
                        progressBar.SetValue(1f);
                        progressBar.gameObject.SetActive(true);
                    }
                
                    // 计算0.3-1秒之间的进度(0-1)
                    float progressTime = pressDuration - ShortPressThreshold; // 0到0.7
                    float progress = progressTime / (LongPressThreshold - ShortPressThreshold); // 0到1
                    progressBar.SetValue(1f - progress); // 从1降到0
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
        
            // 短按逻辑（只在释放时且未达到长按阈值时执行）
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
            var hasEnoughMoney = CheckPlayerMoney(price);
            
            if (hasEnoughMoney)
            {
                DeductPlayerMoney(playerID, price);
                item.IsPurchase = true;
            
                AudioManager.Instance.Play("购买", _catchBall.transform.position, 0.7f);
            }
            else
            {
                EventBus<UIMessageEvent>.Publish(new UIMessageEvent("资金不足，无法购买！", 2f, UIMessageType.Warning));
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
    
    private bool CheckPlayerMoney(int price)
    {
        return GameController.Instance.GetMoney() >= price;
    }

    private void DeductPlayerMoney(ulong playerId, int amount)
    {
        GameController.Instance.DeductMoney(amount);
    }
}