using UnityEngine;
using System.Collections;

public class MaterialFadeAndDestroy : MonoBehaviour
{
    [Header("材质控制设置")]
    public Material targetMaterial;
    public string cutoffProperty = "Vector1_CFBBCBA"; // 改成你实际Shader使用的属性名
    public float fadeTargetValue = 1f;         // 最终值（通常是完全消散）
    public float fadeSpeed = 1f;               // 越大越快

    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (targetMaterial == null || !targetMaterial.HasProperty(cutoffProperty))
        {
            Debug.LogError("材质或属性无效，Fade 无法进行");
            return;
        }

    }

    public void StartFadeAndDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        float currentValue = targetMaterial.GetFloat(cutoffProperty);
        fadeCoroutine = StartCoroutine(FadeOutCoroutine(currentValue, fadeTargetValue));
    }

    private IEnumerator FadeOutCoroutine(float startValue, float targetValue)
    {
        float elapsedTime = 0f;
        float duration = 1f / Mathf.Max(0.01f, fadeSpeed);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float current = Mathf.Lerp(startValue, targetValue, t);
            targetMaterial.SetFloat(cutoffProperty, current);
            yield return null;
        }

        // 最终设置并销毁物体
        targetMaterial.SetFloat(cutoffProperty, targetValue);
        Destroy(gameObject);
    }
}
