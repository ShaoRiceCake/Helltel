using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class ParentUIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("颜色设置")]
    [Tooltip("父物体默认颜色")] 
    public Color parentNormalColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    [Tooltip("悬停时父物体颜色")] 
    public Color parentHoverColor = Color.white;
    [Tooltip("子物体亮度系数（0.8=变暗20%）")] 
    [Range(0.1f, 1f)] public float childDarkenFactor = 0.8f;
    [Range(0.1f, 1f)] public float colorTransitionTime = 0.3f;

    [Header("缩放动画")]
    [Tooltip("建议值1.02-1.1")] 
    public float hoverScale = 1.05f;
    [Range(0.1f, 1f)] public float scaleAnimationTime = 0.4f;

    [Header("动画曲线")] 
    public Ease animationEase = Ease.OutCubic;

    // 组件引用
    private RectTransform _rect;
    private Image _parentImage;
    private Vector3 _originalScale;

    // 子物体颜色数据
    private List<Graphic> _childGraphics = new List<Graphic>();
    private Dictionary<Graphic, Color> _originalColors = new Dictionary<Graphic, Color>();

    private void Awake()
    {
        // 初始化父物体组件
        _rect = GetComponent<RectTransform>();
        _parentImage = GetComponent<Image>();
        _originalScale = _rect.localScale;
        _parentImage.color = parentNormalColor;

        // 收集所有子物体的Graphic组件
        CacheChildGraphics();

        // 设置子物体初始颜色
        SetChildrenColor(childDarkenFactor);

        // 自动添加事件穿透
        AddEventPassThroughToChildren();
    }

    // 缓存子物体的颜色信息
    private void CacheChildGraphics()
    {
        foreach (Transform child in transform)
        {
            var graphics = child.GetComponentsInChildren<Graphic>(true);
            foreach (var graphic in graphics)
            {
                // 排除父物体自身
                if (graphic.gameObject == gameObject) continue; 

                _childGraphics.Add(graphic);
                _originalColors[graphic] = graphic.color;
            }
        }
    }
    

    // 鼠标进入（包括子物体）
    public void OnPointerEnter(PointerEventData eventData)
    {
        
        AudioManager.Instance.Play("鼠标进入按钮");
        // 父物体动画
        _parentImage.DOColor(parentHoverColor, colorTransitionTime).SetEase(animationEase);
        _rect.DOScale(_originalScale * hoverScale, scaleAnimationTime).SetEase(animationEase);

        // 子物体颜色恢复原始亮度
        SetChildrenColor(1f); 
    }

    // 鼠标离开（包括子物体）
    public void OnPointerExit(PointerEventData eventData)
    {
        
        AudioManager.Instance.Play("鼠标离开按钮");
        // 父物体重置
        _parentImage.DOColor(parentNormalColor, colorTransitionTime).SetEase(animationEase);
        _rect.DOScale(_originalScale, scaleAnimationTime).SetEase(animationEase);

        // 子物体变暗
        SetChildrenColor(childDarkenFactor); 
    }

    // 统一设置子物体颜色（factor=1为原始颜色）
    private void SetChildrenColor(float brightnessFactor)
    {
        foreach (var graphic in _childGraphics)
        {
            if (graphic == null) continue;

            Color targetColor = _originalColors[graphic] * brightnessFactor;
            graphic.DOColor(targetColor, colorTransitionTime)
                   .SetEase(animationEase)
                   .SetLink(graphic.gameObject);
        }
    }

    // 当父UI被禁用时强制重置
    private void OnDisable()
    {
        _rect.localScale = _originalScale;
        _parentImage.color = parentNormalColor;
        SetChildrenColor(childDarkenFactor); // 保持子物体变暗
    }

    // 自动为子物体添加事件穿透
    private void AddEventPassThroughToChildren()
    {
        foreach (Transform child in transform)
        {
            var eventPass = child.gameObject.AddComponent<HoverEventPassThrough>();
            eventPass.parentEffect = this;
        }
    }
}