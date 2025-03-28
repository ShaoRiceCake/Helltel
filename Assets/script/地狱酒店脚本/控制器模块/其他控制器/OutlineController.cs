using UnityEngine;
using cakeslice;

[RequireComponent(typeof(Outline))]
public class OutlineController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("是否在游戏开始时关闭 Outline 高亮")]
    [SerializeField] private bool disableOnStart = true;
    [Tooltip("是否锁定当前状态（锁定后无法修改高亮状态）")]
    [SerializeField] private bool isLocked;

    private Outline _outline;
    private bool _isInitialized;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
        if (_outline == null)
        {
            Debug.LogError("OutlineController: Outline component not found!", this);
            return;
        }
        _isInitialized = true;
    }

    private void Start()
    {
        if (disableOnStart && _isInitialized && !isLocked)
        {
            StartCoroutine(DisableAfterFrame());
        }
    }

    private System.Collections.IEnumerator DisableAfterFrame()
    {
        yield return null;
        _outline.enabled = false;
    }

    // === 外部控制接口 ===
    public void SetOutlineEnabled(bool isEnabled)
    {
        if (!_isInitialized || isLocked) return;
        _outline.enabled = isEnabled;
    }

    public void ToggleOutline()
    {
        if (!_isInitialized || isLocked) return;
        _outline.enabled = !_outline.enabled;
    }

    public bool IsOutlineEnabled()
    {
        return _isInitialized && _outline.enabled;
    }

    // === 锁定/解锁功能 ===
    public void LockOutlineState()
    {
        isLocked = true;
    }

    public void UnlockOutlineState()
    {
        isLocked = false;
    }

    public bool IsLocked()
    {
        return isLocked;
    }
}