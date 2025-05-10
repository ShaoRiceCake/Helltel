using System.Collections;
using UnityEngine;

public class ItemMagicLight : ActiveItem
{
    [Header("Light Settings")]
    [SerializeField] private Light pointLight; // 点光源组件
    [SerializeField] private float intensityIncrease = 5f; // 亮度增加量
    [SerializeField] private float activationTime = 3f; // 激活时间
    [SerializeField] private Color startColor ; // 初始颜色
    [SerializeField] private Color endColor ; // 结束颜色

    [Header("Collider Settings")]
    [SerializeField] private Collider triggerCollider; // 碰撞盒组件
    [SerializeField] private string monsterTag = "Monster"; // 怪物标签

    [Header("Effects")]
    [SerializeField] private GameObject monsterHitEffect; // 击中怪物特效

    private float _originalIntensity; // 原始亮度
    private bool _isActivated; // 是否已激活

    protected override void Awake()
    {
        base.Awake();
        if (pointLight == null) pointLight = GetComponent<Light>();
        if (triggerCollider == null) triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null) triggerCollider.isTrigger = true;

        if (pointLight != null)
        {
            _originalIntensity = pointLight.intensity;
            pointLight.color = startColor; // 初始化颜色
        }
        
        OnUseStart.AddListener(StartUseProcess);
    }

    private void StartUseProcess()
    {
        if (_isActivated) return;
        
        _isActivated = true;
        StartCoroutine(ActivateLight());
    }

    private IEnumerator ActivateLight()
    {
        var elapsedTime = 0f;
        var targetIntensity = _originalIntensity + intensityIncrease;

        while (elapsedTime < activationTime)
        {
            if (pointLight)
            {
                // 同时渐变亮度和颜色
                float progress = elapsedTime / activationTime;
                pointLight.intensity = Mathf.Lerp(_originalIntensity, targetIntensity, progress);
                pointLight.color = Color.Lerp(startColor, endColor, progress);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (pointLight)
        {
            pointLight.intensity = targetIntensity;
            pointLight.color = endColor; // 确保最终颜色准确
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActivated) return;

        if (!other.CompareTag(monsterTag)) return;
        
        var effect = PlayEffect(monsterHitEffect, transform.position);

        if (IsGrabbed)
        {
            SelfDetach(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private static GameObject PlayEffect(GameObject effectPrefab, Vector3 position)
    {
        var effectInstance = Instantiate(
            effectPrefab, 
            position, 
            Quaternion.identity
        );
        return effectInstance;
    }

    private void OnDestroy()
    {
        OnUseStart.RemoveListener(StartUseProcess);
    }
}