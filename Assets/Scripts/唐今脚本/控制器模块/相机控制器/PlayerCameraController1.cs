using UnityEngine;
using Cinemachine;
using System.Collections;

[RequireComponent(typeof(CinemachineManager))]
[RequireComponent(typeof(PlayerControlInformationProcess))]
[RequireComponent(typeof(PlayerFootTrackingController))]
public class PlayerCameraController1 : MonoBehaviour
{
    [SerializeField] private PlayerFootTrackingController footTracker;
    
    [Header("Camera Settings")]
    [Tooltip("所有需要管理的虚拟摄像头数组")]
    public CinemachineVirtualCameraBase[] virtualCameras;

    [Tooltip("初始默认激活的摄像头索引")]
    [Range(0, 3)] public int defaultCameraIndex;
    
    private CinemachineManager _cameraSwitcher;
    private PlayerControlInformationProcess _controlHandler;
    private int _lastCameraIndex;
    private bool _isFootLifted;
    private bool _isSwitchingBaseCamera;
    private Coroutine _switchCooldownCoroutine;
    
    private enum CameraType
    {
        Back = 0,
        Left = 1,
        Front = 2,
        Right = 3,
        LeftFootBack = 4,
        RightFootBack = 5,
        LeftFootLeft = 6,
        RightFootLeft = 7,
        LeftFootFront = 8,
        RightFootFront = 9,
        LeftFootRight = 10,
        RightFootRight = 11
    }

    private void Awake()
    {
        _cameraSwitcher = GetComponent<CinemachineManager>();
        _controlHandler = GetComponent<PlayerControlInformationProcess>();
        footTracker = GetComponent<PlayerFootTrackingController>();
        _lastCameraIndex = defaultCameraIndex;
        
        if (_controlHandler == null)
        {
            Debug.LogError("ControlHandler component not found!");
            return;
        }
        
        _controlHandler.onLiftLeftLeg.AddListener(OnLeftFootLifted);
        _controlHandler.onReleaseLeftLeg.AddListener(OnLeftFootReleased);
        _controlHandler.onLiftRightLeg.AddListener(OnRightFootLifted);
        _controlHandler.onReleaseRightLeg.AddListener(OnRightFootReleased);
    }

    private void OnDestroy()
    {
        if (_controlHandler == null) return;
        
        _controlHandler.onLiftLeftLeg.RemoveListener(OnLeftFootLifted);
        _controlHandler.onReleaseLeftLeg.RemoveListener(OnLeftFootReleased);
        _controlHandler.onLiftRightLeg.RemoveListener(OnRightFootLifted);
        _controlHandler.onReleaseRightLeg.RemoveListener(OnRightFootReleased);
        
        if (footTracker != null)
        {
            footTracker.onFootLocked.RemoveListener(OnFootLocked);
        }

        if (_switchCooldownCoroutine != null)
        {
            StopCoroutine(_switchCooldownCoroutine);
        }
    }

    private void OnEnable()
    {
        if (footTracker != null)
        {
            footTracker.onFootLocked.AddListener(OnFootLocked);
        }
        else
        {
            Debug.LogError("PlayerFootTrackingController not found");
        }
    }
    
    private void OnDisable()
    {
        if (footTracker != null)
        {
            footTracker.onFootLocked.RemoveListener(OnFootLocked);
        }
    }
    
    private void OnFootLocked(FootRegion region)
    {
        if (_isFootLifted || _isSwitchingBaseCamera) return;

        switch (region)
        {
            case FootRegion.Front:
                SwitchToBaseCamera(CameraType.Front);
                break;
            case FootRegion.Back:
                SwitchToBaseCamera(CameraType.Back);
                break;
            case FootRegion.Left:
                SwitchToBaseCamera(CameraType.Left);
                break;
            case FootRegion.Right:
                SwitchToBaseCamera(CameraType.Right);
                break;
        }
    }

    private void SwitchToBaseCamera(CameraType cameraType)
    {
        var newIndex = (int)cameraType;
        if (newIndex == _lastCameraIndex) return; 

        _lastCameraIndex = newIndex;
        _cameraSwitcher.SetActiveCamera(newIndex);
        
        if (_switchCooldownCoroutine != null)
        {
            StopCoroutine(_switchCooldownCoroutine);
        }
        _switchCooldownCoroutine = StartCoroutine(BaseCameraSwitchCooldown());
    }

    private IEnumerator BaseCameraSwitchCooldown()
    {
        _isSwitchingBaseCamera = true;
        yield return new WaitForSeconds(3f);
        _isSwitchingBaseCamera = false;
    }

    private void OnLeftFootLifted()
    {
        _isFootLifted = true;
        var baseStance = GetCurrentBaseStance();
        _cameraSwitcher.SetActiveCamera(GetFootCameraIndex(true, baseStance));
    }

    private void OnLeftFootReleased()
    {
        _isFootLifted = false;
        _cameraSwitcher.SetActiveCamera(_lastCameraIndex);
    }

    private void OnRightFootLifted()
    {
        _isFootLifted = true;
        var baseStance = GetCurrentBaseStance();
        _cameraSwitcher.SetActiveCamera(GetFootCameraIndex(false, baseStance));
    }

    private void OnRightFootReleased()
    {
        _isFootLifted = false;
        _cameraSwitcher.SetActiveCamera(_lastCameraIndex);
    }

    private int GetCurrentBaseStance()
    {
        var currentCamera = _cameraSwitcher.GetCurrentCamera();
        return currentCamera is >= 0 and <= 3 ? currentCamera : _lastCameraIndex;
    }

    private static int GetFootCameraIndex(bool isLeftFoot, int baseStance)
    {
        return baseStance switch
        {
            (int)CameraType.Left => isLeftFoot ? (int)CameraType.LeftFootLeft : (int)CameraType.RightFootLeft,
            (int)CameraType.Back => isLeftFoot ? (int)CameraType.LeftFootBack : (int)CameraType.RightFootBack,
            (int)CameraType.Right => isLeftFoot ? (int)CameraType.LeftFootRight : (int)CameraType.RightFootRight,
            (int)CameraType.Front => isLeftFoot ? (int)CameraType.LeftFootFront : (int)CameraType.RightFootFront,
            _ => isLeftFoot ? (int)CameraType.LeftFootLeft : (int)CameraType.RightFootLeft
        };
    }
}