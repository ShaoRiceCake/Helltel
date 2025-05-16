using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceProgressBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform progressBarUI;
    [SerializeField] public ProgressBarPro progressBarPro;
    
    [Header("Animation Settings")]
    [SerializeField] private Vector3 worldOffset = Vector3.up * 0.5f;
    [SerializeField] private float edgePadding = 50f;
    [SerializeField] private float minViewportSize = 0.3f;
    // [SerializeField] private float appearDuration = 0.3f;
    // [SerializeField] private float disappearDuration = 0.2f;
    
    private Camera mainCamera;
    private Transform targetTransform;
    private Canvas parentCanvas;
    private RectTransform canvasRect;
    private Vector3 originalScale;
    private bool isShowing;
    private float currentPressDuration;
    private Coroutine progressCoroutine;

    private void Awake()
    {
        mainCamera = Camera.main;
        parentCanvas = progressBarUI.GetComponentInParent<Canvas>();
        canvasRect = parentCanvas.GetComponent<RectTransform>();
        originalScale = progressBarUI.localScale;
        
        // 初始隐藏
        progressBarUI.localScale = Vector3.zero;
        progressBarUI.gameObject.SetActive(false);
 
    }

    private void UpdatePosition()
    {
        if (!targetTransform || !mainCamera) return;

        Vector3 worldPosition = targetTransform.position + worldOffset;
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPosition);

        // 处理相机后方的情况
        if (viewportPoint.z < 0)
        {
            viewportPoint.x = 1 - viewportPoint.x;
            viewportPoint.y = 1 - viewportPoint.y;
            viewportPoint.z = 0;
            viewportPoint = Vector3Maxamize(viewportPoint);
        }

        // 计算屏幕位置和缩放
        Vector2 screenPosition = new Vector2(
            Mathf.Clamp(viewportPoint.x * canvasRect.sizeDelta.x, edgePadding, canvasRect.sizeDelta.x - edgePadding),
            Mathf.Clamp(viewportPoint.y * canvasRect.sizeDelta.y, edgePadding, canvasRect.sizeDelta.y - edgePadding)
        );

        float distanceScale = Mathf.Clamp(1 / (viewportPoint.z + 1), minViewportSize, 1f);

        progressBarUI.anchoredPosition = screenPosition - canvasRect.sizeDelta / 2f;
        progressBarUI.localScale = originalScale * distanceScale;
    }

    private Vector3 Vector3Maxamize(Vector3 vector)
    {
        return new Vector3(
            Mathf.Clamp01(vector.x),
            Mathf.Clamp01(vector.y),
            Mathf.Clamp01(vector.z)
        );
    }
}