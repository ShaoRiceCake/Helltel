using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class BloodEffectController : MonoBehaviour
{
    [Tooltip("URP Asset中包含该Renderer Feature的Renderer Data")]
    public UniversalRendererData rendererData;
    
    [Tooltip("效果持续时间(秒)")]
    public float effectDuration = 3f;

    private const string FeatureName = "FullScreenPassRendererFeature";
    private Coroutine _effectCoroutine;
    
    // 单例实例
    private static BloodEffectController _instance;
    
    public static BloodEffectController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BloodEffectController>();
                
                if (_instance == null)
                {
                    GameObject obj = new GameObject("BloodEffectController");
                    _instance = obj.AddComponent<BloodEffectController>();
                    Debug.LogWarning("BloodEffectController实例不存在于场景中，已自动创建。请确保分配必要的引用。");
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // 确保单例唯一性
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        _instance = this;
        
        // 如果希望跨场景保持，取消下面注释
        // DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 全局调用的静态方法，激活血色效果
    /// </summary>
    public static void ActivateBloodEffect()
    {
        Instance.ActivateBloodEffectInstance();
    }

    /// <summary>
    /// 实例方法，实际执行效果激活
    /// </summary>
    public void ActivateBloodEffectInstance()
    {
        if (_effectCoroutine != null)
        {
            StopCoroutine(_effectCoroutine);
        }
        _effectCoroutine = StartCoroutine(BloodEffectRoutine());
    }

    private IEnumerator BloodEffectRoutine()
    {
        // 启用效果
        SetBloodEffectActive(true);
        
        // 等待指定时间
        yield return new WaitForSeconds(effectDuration);
        
        // 禁用效果
        SetBloodEffectActive(false);
        
        _effectCoroutine = null;
    }

    private void SetBloodEffectActive(bool active)
    {
        if (!rendererData)
        {
            Debug.LogError("Renderer Data未分配!");
            return;
        }

        // 查找指定的Renderer Feature
        var feature = rendererData.rendererFeatures.Find(f => f.name == FeatureName);
        if (feature)
        {
            feature.SetActive(active);
            rendererData.SetDirty();
        }
        else
        {
            Debug.LogError($"找不到名为 {FeatureName} 的Renderer Feature!");
        }
    }
}