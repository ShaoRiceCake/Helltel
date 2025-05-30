using UnityEngine;
using Cinemachine;
using System.Collections;

[RequireComponent(typeof(CinemachineManager))]
[RequireComponent(typeof(PlayerControlInformationProcess))]
[RequireComponent(typeof(PlayerControl_HandControl))]
public class PlayerCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("所有需要管理的虚拟摄像头数组")]
    public CinemachineVirtualCameraBase[] virtualCameras;

    [Tooltip("初始默认激活的摄像头索引")]
    [Range(0, 3)] public int defaultCameraIndex;

    [Header("Input Settings")]
    public KeyCode nextCameraKey = KeyCode.D;
    public KeyCode previousCameraKey = KeyCode.A;
    public KeyCode skipNextCameraKey = KeyCode.W;
    public KeyCode skipPreviousCameraKey = KeyCode.S;
    public float inputCooldown = 0.2f; // 新增：输入冷却时间

    private CinemachineManager _cameraSwitcher;
    private int _lastCameraIndex;
    private PlayerControlInformationProcess _controlHandler;
    private PlayerControl_HandControl _handControl;        
    private bool _canInput = true; // 新增：输入控制标志
        
    private const int BaseCameraCount = 4;
    private enum CameraType
    {
        Back = 0,
        Left = 1,
        Front = 2,
        Right = 3,
        LeftFootLeft = 4,
        RightFootLeft = 5,
        LeftFootBack = 6,
        RightFootBack = 7,
        LeftFootRight = 8,
        RightFootRight = 9,
        LeftFootFront = 10,
        RightFootFront = 11,
        HandLeft = 12,
        HandBack = 13,
        HandRight = 14,
        HandFront = 15
    }

    private void Awake()
    {
        _cameraSwitcher = GetComponent<CinemachineManager>();
        _controlHandler = GetComponent<PlayerControlInformationProcess>();
        _handControl = GetComponent<PlayerControl_HandControl>();
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
    }

    private void Update()
    {
        HandleCameraInput();
        HandleHandCamera();
    }

    private void HandleCameraInput()
    {
        if (!_canInput) return; // 如果处于冷却时间则不处理输入

        if (Input.GetKeyDown(nextCameraKey))
        {
            SwitchToNextCamera(1);
            StartCoroutine(InputCooldown());
        }
        else if (Input.GetKeyDown(previousCameraKey))
        {
            SwitchToNextCamera(-1);
            StartCoroutine(InputCooldown());
        }
        else if (Input.GetKeyDown(skipNextCameraKey))
        {
            SwitchToNextCamera(2);
            StartCoroutine(InputCooldown());
        }
        else if (Input.GetKeyDown(skipPreviousCameraKey))
        {
            SwitchToNextCamera(-2);
            StartCoroutine(InputCooldown());
        }
    }

    private IEnumerator InputCooldown()
    {
        _canInput = false;
        yield return new WaitForSeconds(inputCooldown);
        _canInput = true;
    }

    private void SwitchToNextCamera(int step)
    {
        var currentIndex = _cameraSwitcher.GetCurrentCamera();

        switch (currentIndex)
        {
            case >= (int)CameraType.HandLeft and <= (int)CameraType.HandFront:
            {
                var newIndex = currentIndex + step;

                newIndex = newIndex switch
                {
                    > (int)CameraType.HandFront => (int)CameraType.HandLeft,
                    < (int)CameraType.HandLeft => (int)CameraType.HandFront,
                    _ => newIndex
                };

                _cameraSwitcher.SetActiveCamera(newIndex);
                _lastCameraIndex = newIndex - (int)CameraType.HandLeft;
                break;
            }
            case < BaseCameraCount:
            {
                var newIndex = CalculateNewIndex(currentIndex, step);
                _lastCameraIndex = currentIndex;
                _cameraSwitcher.SetActiveCamera(newIndex);
                break;
            }
        }
    }

    private void SwitchHandCamera(int currentHandIndex, int step)
    {
        var baseStance = currentHandIndex - (int)CameraType.HandLeft;
        var newBaseStance = CalculateNewIndex(baseStance, step);
        var newHandIndex = (int)CameraType.HandLeft + newBaseStance;
        _cameraSwitcher.SetActiveCamera(newHandIndex);
        _lastCameraIndex = newBaseStance;
    }

    private static int CalculateNewIndex(int currentIndex, int step)
    {
        var newIndex = (currentIndex + step) % BaseCameraCount;
        if (newIndex < 0) newIndex += BaseCameraCount;
        return newIndex;
    }

    private int GetCurrentBaseStance()
    {
        var currentCamera = _cameraSwitcher.GetCurrentCamera();
        return currentCamera >= BaseCameraCount ? _lastCameraIndex : currentCamera;
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

    private static int GetHandCameraIndex(int baseStance)
    {
        return baseStance switch
        {
            (int)CameraType.Left => (int)CameraType.HandLeft,
            (int)CameraType.Back => (int)CameraType.HandBack,
            (int)CameraType.Right => (int)CameraType.HandRight,
            (int)CameraType.Front => (int)CameraType.HandFront,
            _ => (int)CameraType.HandLeft
        };
    }

    private void OnLeftFootLifted()
    {
        var baseStance = GetCurrentBaseStance();
        _lastCameraIndex = baseStance;
        _cameraSwitcher.SetActiveCamera(GetFootCameraIndex(true, baseStance));
    }

    private void OnLeftFootReleased()
    {
        _cameraSwitcher.SetActiveCamera(_lastCameraIndex);
    }

    private void OnRightFootLifted()
    {
        var baseStance = GetCurrentBaseStance();
        _lastCameraIndex = baseStance;
        _cameraSwitcher.SetActiveCamera(GetFootCameraIndex(false, baseStance));
    }

    private void OnRightFootReleased()
    {
        _cameraSwitcher.SetActiveCamera(_lastCameraIndex);
    }
    
    private void HandleHandCamera()
    {
        var currentCamera = _cameraSwitcher.GetCurrentCamera();
        var nowHand = _handControl.CurrentHand;

        if (nowHand != 0)
        {
            if (currentCamera is >= BaseCameraCount and < (int)CameraType.HandLeft) return;
            var baseStance = GetCurrentBaseStance();
            _lastCameraIndex = baseStance;
            _cameraSwitcher.SetActiveCamera(GetHandCameraIndex(baseStance));
        }
        else if (currentCamera >= (int)CameraType.HandLeft)
        {
            _cameraSwitcher.SetActiveCamera(_lastCameraIndex);
        }
    }
}