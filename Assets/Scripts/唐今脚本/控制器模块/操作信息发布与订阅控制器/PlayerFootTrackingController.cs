using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerControlInformationProcess))]
public class PlayerFootTrackingController : MonoBehaviour
{
    [Header("References")]
    public GameObject headObject;
    
    [System.Serializable]
    public class FootLockEvent : UnityEvent<FootRegion> {}
    
    private PlayerControlInformationProcess _controlInfoProcess;
    private bool _trackingRightFoot;
    private bool _trackingLeftFoot;
    private PlayerControl_RightFootControl _rightFoot;
    private PlayerControl_LeftFootControl _leftFoot;
    public FootLockEvent onFootLocked = new FootLockEvent();
    private void Start()
    {
        _controlInfoProcess = GetComponent<PlayerControlInformationProcess>();
        if (!_controlInfoProcess)
        {
            Debug.LogError("No player control info process found!");
            return;
        }
        StartCoroutine(DelayedSubscription());
    }

    private IEnumerator DelayedSubscription()
    {
        yield return null;

        _rightFoot = GetComponent<PlayerControl_RightFootControl>();
        _leftFoot = GetComponent<PlayerControl_LeftFootControl>();

        if (!_rightFoot || !_leftFoot)
        {
            Debug.LogError("Missing foot controls! RightFoot: " + (_rightFoot) + 
                         ", LeftFoot: " + (_leftFoot));
            yield break;
        }

        _controlInfoProcess.onLiftRightLeg.AddListener(StartTrackingRightFoot);
        _controlInfoProcess.onLiftLeftLeg.AddListener(StartTrackingLeftFoot);
        _rightFoot.onFootLocked.AddListener(OnRightFootLocked);
        _leftFoot.onFootLocked.AddListener(OnLeftFootLocked);
    }

    private void OnDisable()
    {
        if (_controlInfoProcess == null) return;
        
        _controlInfoProcess.onLiftRightLeg.RemoveListener(StartTrackingRightFoot);
        _controlInfoProcess.onLiftLeftLeg.RemoveListener(StartTrackingLeftFoot);
        
        if (_rightFoot != null) _rightFoot.onFootLocked.RemoveListener(OnRightFootLocked);
        if (_leftFoot != null) _leftFoot.onFootLocked.RemoveListener(OnLeftFootLocked);
    }

    private void StartTrackingRightFoot()
    {
        _trackingRightFoot = true;
        _trackingLeftFoot = false;
    }

    private void StartTrackingLeftFoot()
    {
        _trackingLeftFoot = true;
        _trackingRightFoot = false;
    }

    private void OnRightFootLocked(Vector3 footPosition)
    {
        if (!_trackingRightFoot) return;
        LogFootPosition(footPosition);
        _trackingRightFoot = false;
    }

    private void OnLeftFootLocked(Vector3 footPosition)
    {
        if (!_trackingLeftFoot) return;
        LogFootPosition(footPosition);
        _trackingLeftFoot = false;
    }

    private void LogFootPosition(Vector3 footPosition)
    {
        if (!headObject)
        {
            Debug.LogError("Head object reference is missing!");
            return;
        }

        var relativePosition = footPosition - headObject.transform.position;
        var headForward = headObject.transform.forward;
        var headRight = headObject.transform.right;

        relativePosition.y = 0;
        headForward.y = 0;
        headRight.y = 0;

        headForward.Normalize();
        headRight.Normalize();

        var forwardDot = Vector3.Dot(relativePosition, headForward);
        var rightDot = Vector3.Dot(relativePosition, headRight);

        var region = DetermineRegion(forwardDot, rightDot);
        onFootLocked?.Invoke(region);
    }

    private static FootRegion DetermineRegion(float forwardDot, float rightDot)
    {
        if (Mathf.Abs(forwardDot) > Mathf.Abs(rightDot))
        {
            return forwardDot > 0 ? FootRegion.Back : FootRegion.Front;
        }
        return rightDot > 0 ? FootRegion.Left : FootRegion.Right;
    }
}