using UnityEngine;
using UnityEngine.EventSystems;

public class HoverEventPassThrough : MonoBehaviour, 
    IPointerEnterHandler, 
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler // 保持滑块等组件的拖拽功能
{
    [HideInInspector] public ParentUIHoverEffect parentEffect;

    // 鼠标进入子元素时触发父级效果
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (parentEffect != null)
            parentEffect.OnPointerEnter(eventData);
    }

    // 鼠标离开子元素时触发父级恢复
    public void OnPointerExit(PointerEventData eventData)
    {
        if (parentEffect != null)
            parentEffect.OnPointerExit(eventData);
    }

    //===== 保持子元素的功能 =====//
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 透传拖拽事件给可拖拽组件
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
    }
}