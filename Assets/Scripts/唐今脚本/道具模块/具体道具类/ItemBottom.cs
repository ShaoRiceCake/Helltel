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

        if (blendShapeCoroutine != null)
        {
            StopCoroutine(blendShapeCoroutine);
        }

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

            for (int i = 0; i < blendShapeCount; i++)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(i, blendValue);
            }

            yield return null;
        }

        for (int i = 0; i < blendShapeCount; i++)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(i, 100f);
        }

        blendShapeCoroutine = null;
    }

    private void OnDestroy()
    {
        OnUseStart.RemoveListener(StartUseProcess);

        if (blendShapeCoroutine != null)
        {
            StopCoroutine(blendShapeCoroutine);
        }
    }
}