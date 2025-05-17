using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using Michsky.LSS;

public class MainMenu : MonoBehaviour
{
    [Header("3D 按钮设置")]
    public GameObject[] menuButtons; // 所有3D菜单按钮
    public Material defaultMaterial; // 默认材质
    public Material hoverMaterial;   // 悬停材质
    
    
    [Header("面板设置")]
    public GameObject onlineMultiplayerPanel; // 加入房间面板
    public GameObject TutorialPanel; // 教程面板
    
    [Header("其他组件")]
    public GlobalUIController globalUIController;
    public LoadingScreenManager lSS_Manager;

    
    
    private RaycastHit hit;
    private GameObject currentHoveredButton;
    [Header("层级设置")]
    public LayerMask buttonLayerMask ; // 在Inspector中设置为只包含3D按钮层
    [Header("相机跟随设置")]
    private Camera mainCamera;
    [SerializeField] private float edgeThreshold = 0.2f; // 屏幕边缘阈值(0-1)
    [SerializeField] private float rotationSpeed = 0.5f; // 旋转速度
    [SerializeField] private float maxPitchAngle = 3f;   // 最大俯仰角度
    [SerializeField] private float maxYawAngle = 6f;     // 最大偏航角度
    
    private Vector3 originalCameraRotation; // 相机初始旋转

    private void Start()
    {
        Debug.Log("MainMenu脚本初始化开始");

        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("主相机未找到！");
        else Debug.Log("主相机获取成功");
        // 记录相机初始旋转
        if(mainCamera != null)
        {
            originalCameraRotation = mainCamera.transform.eulerAngles;
        }

        onlineMultiplayerPanel.SetActive(false);
        TutorialPanel.SetActive(false);

        Debug.Log($"已注册按钮数量: {menuButtons.Length}");
        foreach (var btn in menuButtons)
        {
            if (btn == null) Debug.LogWarning("菜单按钮数组中存在空引用！");
            else Debug.Log($"注册按钮: {btn.name}");
        }
        AudioManager.Instance.Play("开头结尾环境音", loop: true);
        AudioManager.Instance.Play("平静氛围环境音", loop: true);
    }

    private void Update()
    {
        HandleButtonHover();
        HandleButtonClick();
        HandleCameraMovement();
    }
    /// <summary>
    /// 处理相机跟随鼠标移动
    /// </summary>
    private void HandleCameraMovement()
    {
        if(mainCamera == null || onlineMultiplayerPanel.activeSelf || TutorialPanel.activeSelf)
            return;
        
        // 获取鼠标在屏幕上的位置(0-1)
        Vector2 mousePos = new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height);
        
        // 计算鼠标位置与屏幕中心的偏移量(-1到1)
        Vector2 offset = new Vector2(
            (mousePos.x - 0.5f) * 2f,
            (mousePos.y - 0.5f) * 2f);
        
        // 只在鼠标靠近边缘时移动相机
        if(Mathf.Abs(offset.x) < edgeThreshold && Mathf.Abs(offset.y) < edgeThreshold)
            return;
        
        // 计算目标旋转角度(基于初始旋转)
        float targetYaw = originalCameraRotation.y + offset.x * maxYawAngle;
        float targetPitch = originalCameraRotation.x - offset.y * maxPitchAngle;
        
        // 平滑旋转
        float currentYaw = Mathf.LerpAngle(
            mainCamera.transform.eulerAngles.y, 
            targetYaw, 
            rotationSpeed * Time.deltaTime);
        
        float currentPitch = Mathf.LerpAngle(
            mainCamera.transform.eulerAngles.x, 
            targetPitch, 
            rotationSpeed * Time.deltaTime);
        
        // 应用旋转
        mainCamera.transform.eulerAngles = new Vector3(
            currentPitch,
            currentYaw,
            originalCameraRotation.z);
    }
    
    /// <summary>
    /// 处理按钮悬停效果
    /// </summary>
    private void HandleButtonHover()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            // 检查是否是菜单按钮
            if (Array.Exists(menuButtons, button => button == hitObject))
            {
                // 新悬停的按钮
                if (currentHoveredButton != hitObject)
                {
                    // 恢复之前悬停按钮的材质
                    if (currentHoveredButton != null)
                    {
                        SetButtonMaterial(currentHoveredButton, defaultMaterial);
                    }

                    // 设置新悬停按钮的材质
                    currentHoveredButton = hitObject;
                    AudioManager.Instance.Play("泡泡音");
                    SetButtonMaterial(currentHoveredButton, hoverMaterial);
                }
            }
            else
            {
                // 鼠标不在任何按钮上
                if (currentHoveredButton != null)
                {
                    SetButtonMaterial(currentHoveredButton, defaultMaterial);
                    currentHoveredButton = null;
                }
            }
        }
        else
        {
            // 鼠标不在任何按钮上
            if (currentHoveredButton != null)
            {
                SetButtonMaterial(currentHoveredButton, defaultMaterial);
                currentHoveredButton = null;
            }
        }
    }
    
    /// <summary>
    /// 处理按钮点击
    /// </summary>
    private void HandleButtonClick()
    {
        if (Input.GetMouseButtonDown(0) && currentHoveredButton != null)
        {
            // 按钮点击动画 - 移动位置
            StartCoroutine(AnimateButtonClick(currentHoveredButton));
            
            // 根据按钮名称执行相应操作
            switch (currentHoveredButton.name)
            {
                case "SinglePlayerButton":
                    SinglePlayerButton();
                    break;
                case "OpenOnlineMultiplayerButton":
                    OpenOnlineMultiplayerPanel();
                    break;
                case "TutorialButton":
                    OpenTutorialPanel();
                    break;
                case "SettingButton":
                    globalUIController.OpenSettings();
                    break;
                case "GuestBookButton":
                    globalUIController.OpenGuestBook();
                    break;
            }
        }
    }
    
    /// <summary>
    /// 设置按钮材质
    /// </summary>
    private void SetButtonMaterial(GameObject button, Material material)
    {
        Renderer renderer = button.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
    }
    
    /// <summary>
    /// 按钮点击动画
    /// </summary>
    private System.Collections.IEnumerator AnimateButtonClick(GameObject button)
    {
        Vector3 originalPosition = button.transform.position;
        Vector3 pressedPosition = originalPosition - new Vector3(0, 0.01f, 0); // 向下移动一点
        
        // 按下效果
        button.transform.position = pressedPosition;
        
        // 等待短暂时间
        yield return new WaitForSeconds(0.1f);
        
        // 恢复原位置
        button.transform.position = originalPosition;
    }
    
    /// <summary>
    /// 创建单人游戏按钮点击事件
    /// </summary>
    public void SinglePlayerButton()
    {
        lSS_Manager.LoadScene();
        SceneManager.LoadScene("单机正式电梯");
    }
    
    /// <summary>
    /// 打开多人联机界面
    /// </summary>
    public void OpenOnlineMultiplayerPanel()
    {
        onlineMultiplayerPanel.SetActive(true);
    }
    
    /// <summary>
    /// 打开教程界面
    /// </summary>
    public void OpenTutorialPanel()
    {
        onlineMultiplayerPanel.SetActive(false);
    }
}