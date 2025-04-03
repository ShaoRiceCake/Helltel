using UnityEngine;
using Cinemachine;

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

    private CinemachineManager _cameraSwitcher;
    private int _lastCameraIndex; // 用于记录上一个相机索引
    private PlayerControlInformationProcess _controlHandler;
    private PlayerControl_HandControl _handControl;        
        
    private const int BaseCameraCount = 4; // 基础身位相机数量
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
        if (Input.GetKeyDown(nextCameraKey))
        {
            SwitchToNextCamera(1);
        }
        else if (Input.GetKeyDown(previousCameraKey))
        {
            SwitchToNextCamera(-1);
        }
        else if (Input.GetKeyDown(skipNextCameraKey))
        {
            SwitchToNextCamera(2);
        }
        else if (Input.GetKeyDown(skipPreviousCameraKey))
        {
            SwitchToNextCamera(-2);
        }
    }

    private void SwitchToNextCamera(int step)
    {
        var currentIndex = _cameraSwitcher.GetCurrentCamera();

        switch (currentIndex)
        {
            // 在手部摄像机状态下
            case >= (int)CameraType.HandLeft and <= (int)CameraType.HandFront:
            {
                // 直接计算新的手部摄像机索引
                var newIndex = currentIndex + step;

                newIndex = newIndex switch
                {
                    // 处理循环逻辑
                    > (int)CameraType.HandFront => (int)CameraType.HandLeft,
                    < (int)CameraType.HandLeft => (int)CameraType.HandFront,
                    _ => newIndex
                };

                _cameraSwitcher.SetActiveCamera(newIndex);
        
                // 更新对应的基础身位索引
                _lastCameraIndex = newIndex - (int)CameraType.HandLeft;
                break;
            }
            // 在基础身位摄像机状态下
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
        // 计算基础身位索引 (0-3)
        int baseStance = currentHandIndex - (int)CameraType.HandLeft;
        int newBaseStance = CalculateNewIndex(baseStance, step);
        
        // 切换到新的手部摄像机
        int newHandIndex = (int)CameraType.HandLeft + newBaseStance;
        _cameraSwitcher.SetActiveCamera(newHandIndex);
        
        // 更新_lastCameraIndex为对应的基础身位
        _lastCameraIndex = newBaseStance;
    }

    private static int CalculateNewIndex(int currentIndex, int step)
    {
        var newIndex = (currentIndex + step) % BaseCameraCount;
        
        if (newIndex < 0)
        {
            newIndex += BaseCameraCount;
        }

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