using System.Collections;
using UnityEngine;

public class ItemWeapon_BoxingGlove : PassiveItem
{
    private const string DissolveHeightProperty = "Vector1_CFBBCBA"; // Cutoff Height 的 Reference
    private const string BaseColorProperty = "Color_87DA54D1"; // Base Color 的 Reference

    [Header("Material Settings")]
    [SerializeField] private Material targetMaterial; // 直接引用的材质（确保是实例化的副本）
    [SerializeField] private Color targetColor = Color.red;
    [SerializeField] private float animationDuration = 3.0f;
    [SerializeField] private GameObject destroyEffectPrefab;

    private Coroutine _animationCoroutine;

    private void Start()
    {
        OnGrabbed.AddListener(HandleWeaponGrabbed);
    }

    private void HandleWeaponGrabbed()
    {
        // 隐藏拳套模型
        var selfRender = GetComponent<Renderer>();
        if (selfRender) selfRender.enabled = false;

        DestroyProcess();

        // 查找人类控制器并设置拳击状态
        var humanController = GameObject.Find("HumanController");
        if (humanController == null)
        {
            Debug.LogWarning("HumanController not found in scene.");
            return;
        }

        var leftHand = humanController.GetComponent<PlayerControl_LeftHandControl>();
        var rightHand = humanController.GetComponent<PlayerControl_RightHandControl>();
        
        if (leftHand != null) leftHand.IsPunching = true;
        if (rightHand != null) rightHand.IsPunching = true;
        
        _animationCoroutine = StartCoroutine(AnimateMaterial());
    }

    private void DestroyProcess()
    {
        if (destroyEffectPrefab == null) return;
        var effect = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        Destroy(effect, 2f); // 2秒后自动销毁特效
    }

    private IEnumerator AnimateMaterial()
    {
        if (!targetMaterial)
        {
            Debug.LogError("Target Material is not assigned!");
            yield break;
        }

        // 检查属性是否存在
        if (!targetMaterial.HasProperty(BaseColorProperty))
        {
            Debug.LogError($"Material does not have property: {BaseColorProperty}");
            yield break;
        }

        if (!targetMaterial.HasProperty(DissolveHeightProperty))
        {
            Debug.LogError($"Material does not have property: {DissolveHeightProperty}");
            yield break;
        }
        
        var timer = 0f;

        // 第一阶段：溶解效果（从 1 降到 0）
        while (timer < animationDuration / 2)
        {
            timer += Time.deltaTime;
            var progress = timer / (animationDuration / 2);
            var dissolveValue = Mathf.Lerp(10f, 0f, progress);
            targetMaterial.SetFloat(DissolveHeightProperty, dissolveValue);
            yield return null;
        }

        timer = 0f;

        // 第二阶段：恢复溶解（从 0 升回 1）
        while (timer < animationDuration / 2)
        {
            timer += Time.deltaTime;
            var progress = timer / (animationDuration / 2);
            var dissolveValue = Mathf.Lerp(0f, 10f, progress);
            targetMaterial.SetFloat(DissolveHeightProperty, dissolveValue);
            yield return null;
        }

        // 确保最终状态正确
        targetMaterial.SetFloat(DissolveHeightProperty, 10f);
        _animationCoroutine = null;
        
        SelfDetach(true);
    }

    private void OnDestroy()
    {
        OnGrabbed.RemoveListener(HandleWeaponGrabbed);
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
    }
}