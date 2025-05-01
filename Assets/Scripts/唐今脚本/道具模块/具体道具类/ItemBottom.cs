using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBottom : ActiveItem
{
    [SerializeField] private float blendShapeDuration = 0.5f; // 可配置的过渡时间
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Coroutine blendShapeCoroutine;

    protected override void Awake()
    {
        base.Awake();

        // 获取SkinnedMeshRenderer组件
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
        {
            Debug.LogWarning("SkinnedMeshRenderer not found on this object!", this);
        }

        OnUseStart.AddListener(StartUseProcess);
    }

    private void StartUseProcess()
    {
        if(IsExhaust) return;

        // 如果已经有混合动画在进行，先停止它
        if (blendShapeCoroutine != null)
        {
            StopCoroutine(blendShapeCoroutine);
        }

        // 开始新的混合动画
        if (skinnedMeshRenderer != null)
        {
            blendShapeCoroutine = StartCoroutine(AnimateBlendShape());
        }

        IsExhaust = true;
    }

    private IEnumerator AnimateBlendShape()
    {
        float timer = 0f;
        int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;

        // 确保有混合形状可用
        if (blendShapeCount == 0)
        {
            Debug.LogWarning("No blend shapes found on the mesh!", this);
            yield break;
        }

        while (timer < blendShapeDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / blendShapeDuration);
            float blendValue = progress * 100f;

            // 设置所有混合形状的值（如果有多个）
            for (int i = 0; i < blendShapeCount; i++)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(i, blendValue);
            }

            yield return null;
        }

        // 确保最终值为100
        for (int i = 0; i < blendShapeCount; i++)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(i, 100f);
        }

        blendShapeCoroutine = null;
    }

    private void OnDestroy()
    {
        OnUseStart.RemoveListener(StartUseProcess);

        // 停止所有可能的协程
        if (blendShapeCoroutine != null)
        {
            StopCoroutine(blendShapeCoroutine);
        }
    }
}