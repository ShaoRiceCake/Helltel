using UnityEngine;

[ExecuteInEditMode] 
public class RaycastTool : MonoBehaviour
{
    private float _rayLength = 10f; 
    private Vector3 _rayDirection = Vector3.down; 
    public LayerMask ignoreLayers; 
    public GameObject rayLauncher;

    private RaycastHit _hitInfo; 
    private bool _isHit ;

    public float RayLength
    {
        get => _rayLength;
        set
        {
            if (_rayLength <= 0)
            {
                Debug.LogWarning("RayLength is zero!");
            }

            _rayLength = value;
        }
    }

    public Vector3 RyDirection
    {
        get => _rayDirection;
        set
        {
            if (RyDirection == Vector3.zero)
            {
                Debug.LogWarning("RyDirection is zero!");
            }

            _rayDirection = value;
        }
    }


    private void Update()
    {
        PerformRaycast();
    }

    private void PerformRaycast()
    {
        Vector3 rayOrigin;
        Vector3 direction;
        if (rayLauncher)
        {
            rayOrigin = rayLauncher.transform.position;
            direction = rayLauncher.transform.TransformDirection(_rayDirection.normalized);

        }
        else
        {
            rayOrigin = transform.position;
            direction = transform.TransformDirection(_rayDirection.normalized);
        }

        _isHit = Physics.Raycast(rayOrigin, direction, out _hitInfo, _rayLength, ~ignoreLayers);
    }

    public Transform GetHitTrans()
    {
        return _isHit ? _hitInfo.transform : null;
    }

    public Vector3 GetHitPoint()
    {
        return _isHit ? _hitInfo.point : Vector3.zero;
    }

    public bool IsHit()
    {
        return _isHit;
    }
    public RaycastHit GetHitInfo()
    {
        return _hitInfo;
    }
}