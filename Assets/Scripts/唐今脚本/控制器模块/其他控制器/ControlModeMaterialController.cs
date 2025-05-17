using UnityEngine;
using System.Collections;

public class ControlModeMaterialController : MonoBehaviour
{
    [System.Serializable]
    public class HandMaterialSettings
    {
        public Material targetMaterial;
        public string cutoffProperty = "Vector1_CFBBCBA";
        public float activeValue = 2f;
        public float inactiveValue = 10f;
        public float transitionSpeed = 1f;
        
        [HideInInspector] public Coroutine transitionCoroutine;
    }

    [System.Serializable]
    public class LegMaterialSettings
    {
        public Material targetMaterial;
        public string cutoffProperty = "Vector1_CFBBCBA";
        public float activeValue = 1f;
        public float inactiveValue = 10f;
        public float transitionSpeed = 1f;
        
        [HideInInspector] public Coroutine transitionCoroutine;
    }

    [System.Serializable]
    public class HatMaterialSettings
    {
        public Material targetMaterial;
        public string cutoffProperty = "Vector1_CFBBCBA";
        public float activeValue = 3f;
        public float inactiveValue = 10f;
        public float transitionSpeed = 1f;
        
        [HideInInspector] public Coroutine transitionCoroutine;
    }

    [Header("Material References")]
    [SerializeField] private HandMaterialSettings handMaterial;
    [SerializeField] private LegMaterialSettings legMaterial;
    [SerializeField] private HatMaterialSettings hatMaterial;

    [Header("Debug Options")]
    [SerializeField] private bool debugDisableAll = false;

    private PlayerControlInformationProcess controlInfo;
    private bool isCameraModeActive = false;
    private bool isPlayerDead = false;
    private GameDataModel _data;

    private void Start()
    {
        controlInfo = GetComponent<PlayerControlInformationProcess>();
        if (!controlInfo)
        {
            Debug.LogError("PlayerControlInformationProcess not found!");
            return;
        }

        // 初始化材质
        InitializeMaterial(handMaterial);
        InitializeMaterial(legMaterial);
        InitializeMaterial(hatMaterial);

        // 订阅事件
        controlInfo.onSwitchControlMode.AddListener(OnControlModeChanged);
        controlInfo.onCameraControl.AddListener(OnCameraModeActivated);
        controlInfo.onStopCameraControl.AddListener(OnCameraModeDeactivated);
        
        if (_data != null)
        {
            _data.OnIsPlayerDiedChangedEvent += PlayerDie;
        }

        // 初始状态更新
        UpdateMaterialStates();
    }

    private void OnDestroy()
    {
        if (controlInfo == null) return;
        
        controlInfo.onSwitchControlMode.RemoveListener(OnControlModeChanged);
        controlInfo.onCameraControl.RemoveListener(OnCameraModeActivated);
        controlInfo.onStopCameraControl.RemoveListener(OnCameraModeDeactivated);
        
        if (_data != null)
        {
            _data.OnIsPlayerDiedChangedEvent -= PlayerDie;
        }
    }

    private void PlayerDie(bool isDead)
    {
        isPlayerDead = isDead;
        UpdateMaterialStates();
    }

    private void InitializeMaterial(HandMaterialSettings settings)
    {
        if (settings.targetMaterial != null && settings.targetMaterial.HasProperty(settings.cutoffProperty))
        {
            settings.targetMaterial.SetFloat(settings.cutoffProperty, settings.inactiveValue);
        }
    }

    private void InitializeMaterial(LegMaterialSettings settings)
    {
        if (settings.targetMaterial != null && settings.targetMaterial.HasProperty(settings.cutoffProperty))
        {
            settings.targetMaterial.SetFloat(settings.cutoffProperty, settings.inactiveValue);
        }
    }

    private void InitializeMaterial(HatMaterialSettings settings)
    {
        if (settings.targetMaterial != null && settings.targetMaterial.HasProperty(settings.cutoffProperty))
        {
            settings.targetMaterial.SetFloat(settings.cutoffProperty, settings.inactiveValue);
        }
    }

    private void OnControlModeChanged()
    {
        if (!isCameraModeActive)
        {
            UpdateMaterialStates();
        }
    }

    private void OnCameraModeActivated()
    {
        isCameraModeActive = true;
        UpdateMaterialStates();
    }

    private void OnCameraModeDeactivated()
    {
        isCameraModeActive = false;
        UpdateMaterialStates();
    }

    private void UpdateMaterialStates()
    {
        if (debugDisableAll || isPlayerDead)
        {
            ResetAllMaterials();
            return;
        }

        if (isCameraModeActive)
        {
            // 摄像机模式 - 只激活帽子材质
            SetMaterialValue(handMaterial, handMaterial.inactiveValue);
            SetMaterialValue(legMaterial, legMaterial.inactiveValue);
            SetMaterialValue(hatMaterial, hatMaterial.activeValue);
        }
        else
        {
            switch (controlInfo.mCurrentControlMode)
            {
                case PlayerControlInformationProcess.ControlMode.HandControl:
                    SetMaterialValue(handMaterial, handMaterial.activeValue);
                    SetMaterialValue(legMaterial, legMaterial.inactiveValue);
                    SetMaterialValue(hatMaterial, hatMaterial.inactiveValue);
                    break;
                
                case PlayerControlInformationProcess.ControlMode.LegControl:
                    SetMaterialValue(handMaterial, handMaterial.inactiveValue);
                    SetMaterialValue(legMaterial, legMaterial.activeValue);
                    SetMaterialValue(hatMaterial, hatMaterial.inactiveValue);
                    break;
            }
        }
    }

    private void SetMaterialValue(HandMaterialSettings settings, float targetValue)
    {
        if (settings.targetMaterial == null || !settings.targetMaterial.HasProperty(settings.cutoffProperty))
            return;

        if (settings.transitionCoroutine != null)
        {
            StopCoroutine(settings.transitionCoroutine);
        }

        settings.transitionCoroutine = StartCoroutine(TransitionMaterial(
            settings.targetMaterial, 
            settings.cutoffProperty, 
            settings.targetMaterial.GetFloat(settings.cutoffProperty), 
            targetValue, 
            settings.transitionSpeed
        ));
    }

    private void SetMaterialValue(LegMaterialSettings settings, float targetValue)
    {
        if (settings.targetMaterial == null || !settings.targetMaterial.HasProperty(settings.cutoffProperty))
            return;

        if (settings.transitionCoroutine != null)
        {
            StopCoroutine(settings.transitionCoroutine);
        }

        settings.transitionCoroutine = StartCoroutine(TransitionMaterial(
            settings.targetMaterial, 
            settings.cutoffProperty, 
            settings.targetMaterial.GetFloat(settings.cutoffProperty), 
            targetValue, 
            settings.transitionSpeed
        ));
    }

    private void SetMaterialValue(HatMaterialSettings settings, float targetValue)
    {
        if (settings.targetMaterial == null || !settings.targetMaterial.HasProperty(settings.cutoffProperty))
            return;

        if (settings.transitionCoroutine != null)
        {
            StopCoroutine(settings.transitionCoroutine);
        }

        settings.transitionCoroutine = StartCoroutine(TransitionMaterial(
            settings.targetMaterial, 
            settings.cutoffProperty, 
            settings.targetMaterial.GetFloat(settings.cutoffProperty), 
            targetValue, 
            settings.transitionSpeed
        ));
    }

    private IEnumerator TransitionMaterial(Material material, string property, float startValue, float targetValue, float speed)
    {
        float elapsedTime = 0f;
        float duration = 1f / Mathf.Max(0.01f, speed);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float currentValue = Mathf.Lerp(startValue, targetValue, t);
            material.SetFloat(property, currentValue);
            yield return null;
        }

        // 确保最终值准确
        material.SetFloat(property, targetValue);
    }

    private void ResetAllMaterials()
    {
        StopAllCoroutines();

        if (handMaterial.targetMaterial != null && handMaterial.targetMaterial.HasProperty(handMaterial.cutoffProperty))
            handMaterial.targetMaterial.SetFloat(handMaterial.cutoffProperty, handMaterial.inactiveValue);
        
        if (legMaterial.targetMaterial != null && legMaterial.targetMaterial.HasProperty(legMaterial.cutoffProperty))
            legMaterial.targetMaterial.SetFloat(legMaterial.cutoffProperty, legMaterial.inactiveValue);
        
        if (hatMaterial.targetMaterial != null && hatMaterial.targetMaterial.HasProperty(hatMaterial.cutoffProperty))
            hatMaterial.targetMaterial.SetFloat(hatMaterial.cutoffProperty, hatMaterial.inactiveValue);
    }
}