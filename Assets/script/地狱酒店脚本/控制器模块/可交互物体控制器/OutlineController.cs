using UnityEngine;
using cakeslice;

[RequireComponent(typeof(Outline))]
public class OutlineController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("是否在游戏开始时关闭 Outline 高亮")]
    [SerializeField] private bool disableOnStart = true;
    
    private Outline _outline;
    private bool _isInitialized ;
    private bool _isLocked ;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        if (disableOnStart && _isInitialized && !_isLocked)
        {
            StartCoroutine(DisableAfterFrame());
        }
    }

    private System.Collections.IEnumerator DisableAfterFrame()
    {
        yield return null; 
        SetOutlineEnabled(false);
    }

    private void Initialize()
    {
        if (_isInitialized) return;
        
        _outline = GetComponent<Outline>();
        if (!_outline)
        {
            Debug.LogError("Outline component missing on " + gameObject.name, this);
            return;
        }
        
        _isInitialized = true;
    }

    public void SetOutlineEnabled(bool isEnabled)
    {
        if (!_isInitialized) Initialize();
        if (!_isInitialized || _isLocked) return;
        
        _outline.enabled = isEnabled;
    }

    public void LockOutline()
    {
        _isLocked = true;
    }

    public void UnlockOutline()
    {
        _isLocked = false;
    }

    public bool IsOutlineEnabled()
    {
        return _isInitialized && _outline.enabled;
    }
}