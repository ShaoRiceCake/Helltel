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
    [SerializeField] private float blendShapeDuration = 2f;
    [Tooltip("回弹动画的持续时间（秒）")]
    [SerializeField] private float reboundDuration = 1f; // 新增：回弹持续时间
    [Tooltip("目标对象（如果为空则使用自身）")]
    [SerializeField] private GameObject targetObject; // 可配置的外部目标对象

    private SkinnedMeshRenderer targetSkinnedMeshRenderer; // 目标对象的SkinnedMeshRenderer
    private Coroutine blendShapeCoroutine;
    private Coroutine reboundCoroutine; // 新增：回弹协程引用
    private GameDataModel _data;
    private int originalLayer; // 存储目标对象原始layer
     
    [SerializeField] private Transform _targetPosition; // 目标位置的Transform

    protected override void Awake()
    {
        base.Awake();

        // 如果没有指定目标对象，则使用自身
        if (targetObject == null)
        {
            targetObject = gameObject;
        }

        // 获取目标对象的SkinnedMeshRenderer
        targetSkinnedMeshRenderer = targetObject.GetComponent<SkinnedMeshRenderer>();
        if (targetSkinnedMeshRenderer == null)
        {
            Debug.LogWarning($"SkinnedMeshRenderer not found on {targetObject.name}!", this);
        }

        OnUseStart.AddListener(StartUseProcess);
    }

    private void OnEnable()
    {
        _data = Resources.Load<GameDataModel>("GameData");
    }

    protected void Start()
    {
        _data.OnFloorChanged += ResetIsExhaust;
    }

    private void FixedUpdate()
    {
        targetObject.layer = gameObject.layer;
    }
    
    private void StartUseProcess()
    {
        if(IsExhaust) return;
        
        // 停止所有正在进行的动画
        if (blendShapeCoroutine != null)
        {
            StopCoroutine(blendShapeCoroutine);
        }
        if (reboundCoroutine != null)
        {
            StopCoroutine(reboundCoroutine);
        }

        if (targetSkinnedMeshRenderer)
        {
            blendShapeCoroutine = StartCoroutine(AnimateBlendShape());
        }

        IsExhaust = true;
    }

    private IEnumerator AnimateBlendShape()
    {
        var timer = 0f;
        var blendShapeCount = targetSkinnedMeshRenderer.sharedMesh.blendShapeCount;

        if (blendShapeCount == 0)
        {
            Debug.LogWarning("No blend shapes found on the target mesh!", this);
            yield break;
        }

        // 按下动画
        while (timer < blendShapeDuration)
        {
            timer += 3 * Time.deltaTime;
            var progress = Mathf.Clamp01(timer / blendShapeDuration);
            var blendValue = progress * 100f;

            for (int i = 0; i < blendShapeCount; i++)
            {
                targetSkinnedMeshRenderer.SetBlendShapeWeight(i, blendValue);
            }

            yield return null;
        }

        // 确保完全按下
        for (var i = 0; i < blendShapeCount; i++)
        {
            targetSkinnedMeshRenderer.SetBlendShapeWeight(i, 100f);
        }

        OnButtonFullyPressed();
        
        // 开始回弹动画
        reboundCoroutine = StartCoroutine(ReboundAnimation());
        
        blendShapeCoroutine = null;
    }

    // 新增：回弹动画协程
    private IEnumerator ReboundAnimation()
    {
        var timer = 0f;
        var blendShapeCount = targetSkinnedMeshRenderer.sharedMesh.blendShapeCount;

        while (timer < reboundDuration)
        {
            timer += Time.deltaTime;
            var progress = Mathf.Clamp01(timer / reboundDuration);
            // 使用平滑的插值函数使回弹更自然
            var blendValue = Mathf.SmoothStep(100f, 0f, progress);

            for (int i = 0; i < blendShapeCount; i++)
            {
                targetSkinnedMeshRenderer.SetBlendShapeWeight(i, blendValue);
            }

            yield return null;
        }

        // 确保完全回弹
        for (var i = 0; i < blendShapeCount; i++)
        {
            targetSkinnedMeshRenderer.SetBlendShapeWeight(i, 0f);
        }

        reboundCoroutine = null;
    }

    private void OnButtonFullyPressed()
    {
        Debug.Log("按钮已完全按下");
        if(_data.CheckPerformance() == true ||_data.CurrentLoadedScene == _data.shop)
        {
            var gameFlow = FindObjectOfType<GameFlow>();
            if(gameFlow)
            {
                gameFlow.LeaveThisFloor();
            }
            else
            {
                Debug.Log("找不到流程脚本");
            }
        }
        else
        {
            EventBus<UIMessageEvent>.Publish(new UIMessageEvent("绩效未达标！", 2f, UIMessageType.Warning));
            IsExhaust = false;
        }
    }

    private void ResetIsExhaust()
    {
        IsExhaust = false;
    }

    private void OnDestroy()
    {
        OnUseStart.RemoveListener(StartUseProcess);
        GameController.Instance._gameData.OnFloorChanged -= ResetIsExhaust;

        if (blendShapeCoroutine != null)
        {
            StopCoroutine(blendShapeCoroutine);
        }
        if (reboundCoroutine != null)
        {
            StopCoroutine(reboundCoroutine);
        }
    }
}