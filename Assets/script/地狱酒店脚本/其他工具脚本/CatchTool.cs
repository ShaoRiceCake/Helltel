using UnityEngine;
using Obi;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class CatchTool : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask floorLayer; 
    public float catchRadiusMultiplier = 1f; 

    [Header("References")]
    public ObiParticleAttachment obiAttachment;
    public GameObject catchDetectCylinder; 

    // 运行时状态
    private SphereCollider _sphereCollider;
    private GameObject _catchBall;
    private bool _isGrabbing;
    public GameObject currentTarget;
    private Transform _catchAimTrans; 
    public GameObject _currentHighlightedObject; // 当前已高亮的物体


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
        //
        // // 优先级1: 检测Floor层对象
        // var floorColliders = Physics.OverlapSphere(
        //     _catchBall.transform.position, 
        //     _sphereCollider.radius * catchRadiusMultiplier, 
        //     floorLayer);
        //
        // if (floorColliders.Length > 0)
        // {
        //     currentTarget = floorColliders[0].gameObject;
        //     return;
        // }

        // 优先级2: 从预选列表中选择最近的
        if (preSelectedObjects.Count > 0)
        {
            currentTarget = preSelectedObjects
                .OrderBy(obj => Vector3.Distance(
                    obj.transform.position, 
                    _catchAimTrans.position))
                .FirstOrDefault();
        }
        else
        {
            currentTarget = null;
        }
    }
    private void HighLightTarget()
    {
        // 如果新目标就是当前已高亮的物体，则不做任何操作
        if (currentTarget == _currentHighlightedObject) return;
    
        // 先取消旧物体的高亮
        if (_currentHighlightedObject)
        {
            var oldOutline = _currentHighlightedObject.GetComponent<Outline>();
            if (oldOutline) oldOutline.enabled = false;
        }
    
        // 高亮新物体
        if (currentTarget)
        {
            var newOutline = currentTarget.GetComponent<Outline>();
            if (!newOutline) return;
            newOutline.enabled = true;
            _currentHighlightedObject = currentTarget; // 更新记录
        }
        else
        {
            _currentHighlightedObject = null; // 没有目标时清空记录
        }
    }
    
    private void HandleInput()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        
        if (currentTarget && !_isGrabbing)
        {
            GrabObject(currentTarget);
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

    }

    private void ReleaseObject()
    {
        _isGrabbing = false;
        
        obiAttachment.enabled = false;
        obiAttachment.BindToTarget(null);
    }

}