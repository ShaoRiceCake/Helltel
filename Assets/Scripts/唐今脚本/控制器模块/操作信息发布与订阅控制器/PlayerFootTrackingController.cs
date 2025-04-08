using UnityEngine;
using System;

[RequireComponent(typeof(PlayerControlInformationProcess))]
public class PlayerFootTrackingController : MonoBehaviour
{
    [Header("References")]
    public GameObject headObject;
    
    private PlayerControlInformationProcess _controlInfoProcess;
    private bool _trackingRightFoot;
    private bool _trackingLeftFoot;

    private void Start()
    {
        _controlInfoProcess = GetComponent<PlayerControlInformationProcess>();
        if (!_controlInfoProcess)
        {
            Debug.LogError("No player control info process found!");
        }
    }

    private void OnEnable()
    {
        if (_controlInfoProcess == null) return;
        _controlInfoProcess.onLiftRightLeg.AddListener(StartTrackingRightFoot);
        _controlInfoProcess.onLiftLeftLeg.AddListener(StartTrackingLeftFoot);
            
        var rightFoot = GetComponent<PlayerControl_RightFootControl>();
        var leftFoot = GetComponent<PlayerControl_LeftFootControl>();
            
        if (rightFoot != null) rightFoot.onFootLocked.AddListener(OnRightFootLocked);
        if (leftFoot != null) leftFoot.onFootLocked.AddListener(OnLeftFootLocked);
    }

    private void OnDisable()
    {
        if (_controlInfoProcess == null) return;
        _controlInfoProcess.onLiftRightLeg.RemoveListener(StartTrackingRightFoot);
        _controlInfoProcess.onLiftLeftLeg.RemoveListener(StartTrackingLeftFoot);
            
        var rightFoot = GetComponent<PlayerControl_RightFootControl>();
        var leftFoot = GetComponent<PlayerControl_LeftFootControl>();
            
        if (rightFoot != null) rightFoot.onFootLocked.RemoveListener(OnRightFootLocked);
        if (leftFoot != null) leftFoot.onFootLocked.RemoveListener(OnLeftFootLocked);
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
        LogFootPosition(footPosition, "Right");
        _trackingRightFoot = false;
    }

    private void OnLeftFootLocked(Vector3 footPosition)
    {
        if (!_trackingLeftFoot) return;
        LogFootPosition(footPosition, "Left");
        _trackingLeftFoot = false;
    }

    private void LogFootPosition(Vector3 footPosition, string footSide)
    {
        if (headObject == null)
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
        Debug.Log($"{footSide} foot locked in {region} region relative to head");
    }

    private static string DetermineRegion(float forwardDot, float rightDot)
    {
        if (Mathf.Abs(forwardDot) > Mathf.Abs(rightDot))
        {
            return forwardDot > 0 ? "Front" : "Back";
        }
        else
        {
            return rightDot > 0 ? "Right" : "Left";
        }
    }
}