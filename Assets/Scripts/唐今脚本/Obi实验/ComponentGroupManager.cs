using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class ComponentGroupManager : MonoBehaviour
{
    [System.Serializable]
    public class ComponentGroup
    {
        [Tooltip("拖入需要控制的组件或游戏对象")]
        public Object[] components;
        
        [Tooltip("切换时触发的事件")]
        public UnityEvent onActivated;
        
        [Tooltip("切换时触发的事件")]
        public UnityEvent onDeactivated;

        public void SetActive(bool active)
        {
            foreach (var obj in components)
            {
                if (obj == null) continue;

                // 处理不同类型的组件
                switch (obj)
                {
                    case Behaviour behaviour:
                        behaviour.enabled = active;
                        break;
                    case GameObject gameObj:
                        gameObj.SetActive(active);
                        break;
                    case Renderer renderer:
                        renderer.enabled = active;
                        break;
                    case Collider collider:
                        collider.enabled = active;
                        break;
                    // 可以继续添加其他类型的处理
                }
            }

            // 触发事件
            if (active) onActivated.Invoke();
            else onDeactivated.Invoke();
        }
    }

    [Header("组件组设置")]
    public ComponentGroup group1;
    public ComponentGroup group2;

    [Header("切换设置")]
    public KeyCode toggleKey = KeyCode.V;
    public bool startWithGroup1 = true;

    private bool isGroup1Active;

    void Start()
    {
        isGroup1Active = startWithGroup1;
        UpdateGroups();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleGroups();
        }
    }

    private void ToggleGroups()
    {
        isGroup1Active = !isGroup1Active;
        UpdateGroups();
    }

    private void UpdateGroups()
    {
        group1.SetActive(isGroup1Active);
        group2.SetActive(!isGroup1Active);
    }

    // 编辑器工具：查找所有子对象中的组件
    [ContextMenu("查找子对象中的组件")]
    private void FindComponentsInChildren()
    {
        group1.components = GetComponentsInChildren<Component>(true);
        group2.components = new Object[0];
        Debug.Log($"已找到 {group1.components.Length} 个组件");
    }
}