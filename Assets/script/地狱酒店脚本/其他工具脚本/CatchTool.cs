using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CatchTool : MonoBehaviour
{
    [HideInInspector]
    public bool isCatching = true;

    private SphereCollider _sphereCollider;
    private GameObject _catchBall;
    private bool _isGrabbing;
    private bool _isReleasing;
    private GameObject _grabbedObject;
    private Coroutine _releaseCoroutine;
    private ConfigurableJoint _joint; 

    public GameObject CatchBall
    {
        get => _catchBall;
        set
        {
            _catchBall = value;
            _sphereCollider = _catchBall.GetComponent<SphereCollider>();
            NullCheckerTool.CheckNull(_catchBall, _sphereCollider);
        }
    }

    private void Update()
    {
        if (isCatching)
        {
            HandleCatchInput();
        }
    }

    private void HandleCatchInput()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (_isGrabbing)
        {
            ReleaseObject();
        }
        else
        {
            AttemptGrab();
        }
    }

    private void AttemptGrab()
    {
        var colliders = Physics.OverlapSphere(_sphereCollider.transform.position, _sphereCollider.radius);
        var validObjects = (from collider in colliders where !collider.CompareTag("Uncatchable") && !IsChildOfUncatchable(collider.transform) select collider.gameObject).ToList();

        if (validObjects.Count <= 0) return;
        var targetObject = SelectTargetObject(validObjects);
        GrabObject(targetObject);
    }

    private static bool IsChildOfUncatchable(Transform transform)
    {
        while (transform.parent != null)
        {
            if (transform.parent.CompareTag("Uncatchable"))
            {
                return true;
            }
            transform = transform.parent;
        }
        return false;
    }

    private static GameObject SelectTargetObject(List<GameObject> objects)
    {
        GameObject floorObject = null;
        GameObject otherObject = null;

        foreach (var obj in objects)
        {
            if (obj.layer == LayerMask.NameToLayer("Floor"))
            {
                floorObject = obj;
            }
            else
            {
                otherObject = obj;
            }
        }

        return otherObject != null ? otherObject : floorObject;
    }

    private void GrabObject(GameObject targetObject)
    {
        _grabbedObject = targetObject;
        _isGrabbing = true;

        // 创建 ConfigurableJoint
        _joint = _catchBall.AddComponent<ConfigurableJoint>();
        _joint.connectedBody = _grabbedObject.GetComponent<Rigidbody>();

        // 配置约束
        ConfigureJoint(_joint);
    }

    private void ConfigureJoint(ConfigurableJoint joint)
    {
        // 设置为双向约束
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        // 设置强力约束
        joint.breakForce = Mathf.Infinity; // 永不断裂
        joint.breakTorque = Mathf.Infinity; // 永不断裂
    }

    private void ReleaseObject()
    {
        if (_isReleasing)
        {
            return;
        }

        _isReleasing = true;
        _releaseCoroutine = StartCoroutine(ReleaseCoroutine());
    }

    private IEnumerator ReleaseCoroutine()
    {
        // 销毁约束
        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
        }

        _isGrabbing = false;

        var startTime = Time.time;
        while (Time.time - startTime < 1f)
        {
            if (!_sphereCollider.bounds.Intersects(_grabbedObject.GetComponent<Collider>().bounds))
            {
                break;
            }
            yield return null;
        }

        _grabbedObject = null;
        _isReleasing = false;
    }

    private void OnDisable()
    {
        if (_releaseCoroutine == null) return;
        StopCoroutine(_releaseCoroutine);
        _isReleasing = false;
    }
}