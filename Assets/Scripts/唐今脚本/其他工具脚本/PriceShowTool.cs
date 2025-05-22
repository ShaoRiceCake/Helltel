using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PriceShowTool : MonoBehaviour
{
    // 定义状态枚举
    public enum PurchaseState
    {
        Purchasable,
        NonPurchasable
    }

    [Header("TextMeshPro References")]
    [SerializeField] private TextMeshProUGUI purchasableText;  // 可购买状态文本
    [SerializeField] private TextMeshProUGUI nonPurchasableText; // 不可购买状态文本

    [Header("Current State")]
    [SerializeField] private PurchaseState currentState = PurchaseState.Purchasable;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f; // 自转速度（度/秒）
    [SerializeField] private bool rotateAroundY = true; // 是否绕Y轴旋转

    [Header("Follow Settings")]
    [SerializeField] private Transform targetObject; // 要跟随的目标物体
    [SerializeField] private float followSmoothness = 5f; // 跟随平滑度
    
    private Canvas _parentCanvas;
    private GraphicRaycaster _raycaster;

    private void Awake()
    {
        // 获取父Canvas
        _parentCanvas = GetComponent<Canvas>();
        // 初始化状态
        SetState(currentState);
        _raycaster = GetComponent<GraphicRaycaster>();
    }

    private void Start()
    {
        // 更可靠的方式获取主相机
        var mainCamera = Camera.main;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!mainCamera)
        {
            // 如果没有标记为MainCamera的相机，尝试查找玩家相机
            if (player)
            {
                mainCamera = player.GetComponentInChildren<Camera>();
            }
        }
        
        if (_parentCanvas && mainCamera)
        {
            _parentCanvas.worldCamera = mainCamera;
        }
        else
        {
            Debug.LogWarning("无法找到主相机或Canvas未设置");
        }

        var tool = FindFirstObjectByType<CatchDetectorTool>();

        tool.priceShowPrefab = this;
    }
    private void FixedUpdate()
    {
        // 绕Y轴自转
        if (rotateAroundY)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
        
        // 跟随目标物体
        if (!targetObject) return;
        var targetPosition = targetObject.position + Vector3.up;
        transform.position = Vector3.Lerp(
            transform.position, 
            targetPosition,
            followSmoothness * Time.deltaTime
        );
    }

    
    /// <summary>
    /// 设置要跟随的目标物体
    /// </summary>
    public void SetTarget(Transform target)
    {
        targetObject = target;
        if (target)
        {
            // 立即设置初始位置
            transform.position = target.position + Vector3.up;
        }
    }

    /// <summary>
    /// 设置当前状态
    /// </summary>
    /// <param name="newState">新状态</param>
    public void SetState(PurchaseState newState)
    {
        currentState = newState;
        UpdateTextVisibility();
    }

    /// <summary>
    /// 设置显示文本内容
    /// </summary>
    /// <param name="text">要显示的文本</param>
    public void SetText(string text)
    {
        purchasableText.text = text;
        nonPurchasableText.text = text;
    }

    /// <summary>
    /// 设置自转速度
    /// </summary>
    /// <param name="speed">新的自转速度（度/秒）</param>
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    /// <summary>
    /// 设置是否绕Y轴旋转
    /// </summary>
    /// <param name="enabled">是否启用Y轴旋转</param>
    public void SetRotateAroundY(bool enabled)
    {
        rotateAroundY = enabled;
    }

    /// <summary>
    /// 更新文本可见性
    /// </summary>
    private void UpdateTextVisibility()
    {
        // 确保两个文本不会同时显示
        purchasableText.gameObject.SetActive(currentState == PurchaseState.Purchasable);
        nonPurchasableText.gameObject.SetActive(currentState == PurchaseState.NonPurchasable);
    }

    /// <summary>
    /// 获取当前状态
    /// </summary>
    public PurchaseState GetCurrentState()
    {
        return currentState;
    }
}