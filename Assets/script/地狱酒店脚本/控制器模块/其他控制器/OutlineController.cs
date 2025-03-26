using UnityEngine;
using cakeslice; // 确保 Outline 组件的命名空间正确

[RequireComponent(typeof(Outline))] // 强制要求挂载对象必须有 Outline 组件
public class OutlineController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("是否在游戏开始时关闭 Outline 高亮")]
    [SerializeField] private bool disableOnStart = true;

    private Outline _outline;
    private bool _isInitialized;

    private void Awake()
    {
        // 获取 Outline 组件
        _outline = GetComponent<Outline>();
        if (_outline == null)
        {
            Debug.LogError("OutlineController: 未找到 Outline 组件！", this);
            return;
        }
        _isInitialized = true;
    }

    private void Start()
    {
        if (disableOnStart && _isInitialized)
        {
            // 延迟一帧关闭（避免与其他初始化逻辑冲突）
            StartCoroutine(DisableAfterFrame());
        }
    }

    private System.Collections.IEnumerator DisableAfterFrame()
    {
        yield return null; // 等待一帧
        SetOutlineEnabled(false);
    }

    /// <summary>
    /// 外部控制 Outline 的开启/关闭
    /// </summary>
    public void SetOutlineEnabled(bool isEnabled)
    {
        if (!_isInitialized) return;
        _outline.enabled = isEnabled;
    }

    /// <summary>
    /// 切换 Outline 的开关状态
    /// </summary>
    public void ToggleOutline()
    {
        if (!_isInitialized) return;
        _outline.enabled = !_outline.enabled;
    }

    /// <summary>
    /// 直接获取当前 Outline 的启用状态
    /// </summary>
    public bool IsOutlineEnabled()
    {
        return _isInitialized && _outline.enabled;
    }
}