using UnityEngine;
using Cinemachine;

public class CinemachineManager : MonoBehaviour
{
    [Tooltip("所有需要管理的虚拟摄像头数组")]
    public CinemachineVirtualCameraBase[] virtualCameras;

    [Tooltip("初始默认激活的摄像头索引")]
    public int defaultCameraIndex;

    private int _currentCameraIndex;

    private void Start()
    {
        // 验证输入
        if (virtualCameras == null || virtualCameras.Length == 0)
        {
            Debug.LogError("没有分配虚拟摄像头！");
            return;
        }

        if (defaultCameraIndex < 0 || defaultCameraIndex >= virtualCameras.Length)
        {
            Debug.LogWarning("默认摄像头索引超出范围，将使用0作为默认值");
            defaultCameraIndex = 0;
        }

        // 初始化摄像头状态
        _currentCameraIndex = defaultCameraIndex;
        SwitchCamera(_currentCameraIndex);
    }

    private void Update()
    {
        Debug.Log("current cam: "+ _currentCameraIndex);
    }

    private void SwitchCamera(int newCameraIndex)
    {
        // 确保索引有效
        if (newCameraIndex < 0 || newCameraIndex >= virtualCameras.Length)
        {
            Debug.LogError("无效的摄像头索引: " + newCameraIndex);
            return;
        }

        // 禁用所有摄像头
        foreach (var cam in virtualCameras)
        {
            if (cam != null)
            {
                cam.gameObject.SetActive(false);
            }
        }

        // 启用选中的摄像头
        if (virtualCameras[newCameraIndex] != null)
        {
            virtualCameras[newCameraIndex].gameObject.SetActive(true);
            _currentCameraIndex = newCameraIndex;
        }
        else
        {
            Debug.LogError("摄像头索引 " + newCameraIndex + " 为空！");
        }
    }

    public void SetActiveCamera(int index)
    {
        SwitchCamera(index);
    }

    public int GetCurrentCamera()
    {
        return _currentCameraIndex;
    }
}