using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 底部物品类，继承自ActiveItem，用于处理带有混合形状动画的物品逻辑
/// </summary>
public class ItemBottom : ActiveItem
{
    [Header("动画设置")]
    [Tooltip("混合形状动画过渡的持续时间（秒）")]
    [SerializeField] private float blendShapeDuration = 2f; // 可配置的过渡时间

    private SkinnedMeshRenderer skinnedMeshRenderer; // 引用SkinnedMeshRenderer组件
    private Coroutine blendShapeCoroutine;          // 存储当前运行的协程引用

    /// <summary>
    /// 重写Awake方法，初始化组件和事件监听
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // 调用父类的Awake方法

        // 获取SkinnedMeshRenderer组件
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
        {
            Debug.LogWarning("SkinnedMeshRenderer not found on this object!", this);
        }

        // 添加使用开始事件的监听器
        OnUseStart.AddListener(StartUseProcess);
        
    }
    protected void Start()
    {
        GameController.Instance._gameData.OnFloorChanged += ResetIsExhaust;
        
    }

    /// <summary>
    /// 开始使用物品的处理方法
    /// </summary>
    private void StartUseProcess()
    {
        if(IsExhaust) return; // 如果物品已耗尽，则直接返回

        // 如果已有协程在运行，先停止它
        if (blendShapeCoroutine != null)
        {
            StopCoroutine(blendShapeCoroutine);
        }

        // 如果有SkinnedMeshRenderer组件，开始混合形状动画
        if (skinnedMeshRenderer != null)
        {
            blendShapeCoroutine = StartCoroutine(AnimateBlendShape());
        }

        IsExhaust = true; // 标记物品为已耗尽状态
    }

    /// <summary>
    /// 混合形状动画协程
    /// </summary>
    /// <returns>IEnumerator协程迭代器</returns>
    private IEnumerator AnimateBlendShape()
    {
        float timer = 0f; // 动画计时器
        int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount; // 获取混合形状数量

        // 如果没有混合形状，输出警告并退出协程
        if (blendShapeCount == 0)
        {
            Debug.LogWarning("No blend shapes found on the mesh!", this);
            yield break;
        }

        // 动画循环：在指定时间内平滑过渡
        while (timer < blendShapeDuration)
        {
            timer += 3*Time.deltaTime; // 更新计时器
            float progress = Mathf.Clamp01(timer / blendShapeDuration); // 计算动画进度(0-1)
            float blendValue = progress * 100f; // 转换为混合形状权重(0-100)

            // 更新所有混合形状的权重
            for (int i = 0; i < blendShapeCount; i++)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(i, blendValue);
            }
            

            yield return null; // 等待下一帧
        }

        // 确保动画结束时所有混合形状权重设置为100%
        for (int i = 0; i < blendShapeCount; i++)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(i, 100f);
        }

        // 在这里调用按钮完全按下后的方法
        OnButtonFullyPressed(); 

        blendShapeCoroutine = null; // 重置协程引用
    }

    /// <summary>
    /// 对象销毁时的清理工作
    /// </summary>
    private void OnDestroy()
    {
        // 移除事件监听器
        OnUseStart.RemoveListener(StartUseProcess);
        GameController.Instance._gameData.OnFloorChanged -= ResetIsExhaust;

        // 如果协程还在运行，停止它
        if (blendShapeCoroutine != null)
        {
            StopCoroutine(blendShapeCoroutine);
        }
    }
    /// <summary>
    /// 按钮完全按下后的处理逻辑
    /// </summary>
    private void OnButtonFullyPressed()
    {
        Debug.Log("按钮已完全按下");
        GameFlow gameFlow = FindObjectOfType<GameFlow>();
        if(gameFlow !=null)
        {
            gameFlow.LeaveThisFloor();
        }
        else
        Debug.Log("找不到流程脚本");
    }
    private void ResetIsExhaust()
    {
        IsExhaust = false;
    }
}