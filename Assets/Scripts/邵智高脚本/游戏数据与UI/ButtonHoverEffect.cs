using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("悬停效果")]
    public float hoverScale = 1.2f;      // 悬停时放大倍数
    public float scaleSpeed = 10f;      // 缩放动画速度

    private RectTransform rectTransform;
    private Vector3 originalScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    // 鼠标进入时触发
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 播放音效
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play("鼠标进入按钮");

        // 放大按钮
        StopAllCoroutines();
        StartCoroutine(ScaleButton(hoverScale));
    }

    // 鼠标离开时触发
    public void OnPointerExit(PointerEventData eventData)
    {
        // 播放音效
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play("鼠标离开按钮");
        // 恢复原始大小
        StopAllCoroutines();
        StartCoroutine(ScaleButton(1f));
    }
    private void OnDisable()
    {
        // 强制恢复原始大小（即使父物体被隐藏）
        transform.localScale = originalScale;
    }

    // 平滑缩放动画
    private System.Collections.IEnumerator ScaleButton(float targetScale)
    {
        Vector3 target = originalScale * targetScale;
        while (Vector3.Distance(rectTransform.localScale, target) > 0.01f)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, target, Time.unscaledDeltaTime * scaleSpeed);
            yield return null;
        }
        rectTransform.localScale = target;
    }
}