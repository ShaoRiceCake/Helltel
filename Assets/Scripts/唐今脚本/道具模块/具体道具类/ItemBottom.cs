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
    [Tooltip("目标对象（如果为空则使用自身）")]
    [SerializeField] private GameObject targetObject; // 可配置的外部目标对象

    private SkinnedMeshRenderer targetSkinnedMeshRenderer; // 目标对象的SkinnedMeshRenderer
    private Coroutine blendShapeCoroutine;
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
        
        if (blendShapeCoroutine != null)
        {
            StopCoroutine(blendShapeCoroutine);
        }

        if (targetSkinnedMeshRenderer != null)
        {
            blendShapeCoroutine = StartCoroutine(AnimateBlendShape());
        }

        IsExhaust = true;
    }

    private IEnumerator AnimateBlendShape()
    {
        float timer = 0f;
        int blendShapeCount = targetSkinnedMeshRenderer.sharedMesh.blendShapeCount;

        if (blendShapeCount == 0)
        {
            Debug.LogWarning("No blend shapes found on the target mesh!", this);
            yield break;
        }

        while (timer < blendShapeDuration)
        {
            timer += 3 * Time.deltaTime;
            float progress = Mathf.Clamp01(timer / blendShapeDuration);
            float blendValue = progress * 100f;

            for (int i =  0; i < blendShapeCount; i++)
            {
                targetSkinnedMeshRenderer.SetBlendShapeWeight(i, blendValue);
            }

            yield return null;
        }

        for (int i = 0; i < blendShapeCount; i++)
        {
            targetSkinnedMeshRenderer.SetBlendShapeWeight(i, 100f);
        }

        OnButtonFullyPressed();
        blendShapeCoroutine = null;
    }

    private void OnDestroy()
    {
        OnUseStart.RemoveListener(StartUseProcess);
        GameController.Instance._gameData.OnFloorChanged -= ResetIsExhaust;

        if (blendShapeCoroutine != null)
        {
            StopCoroutine(blendShapeCoroutine);
        }

    }

    private void OnButtonFullyPressed()
    {
        Debug.Log("按钮已完全按下");
        if(_data.CheckPerformance() == true ||_data.CurrentLoadedScene == _data.shop)
        {
            GameFlow gameFlow = FindObjectOfType<GameFlow>();
            if(gameFlow != null)
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
            //ToDo执行未达成绩效的逻辑
            AudioManager.Instance.Play("未达成绩效");
            IsExhaust = false;
        }
    }

    private void ResetIsExhaust()
    {
        IsExhaust = false;
    }
}