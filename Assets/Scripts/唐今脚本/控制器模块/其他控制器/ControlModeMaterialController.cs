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
        
        [HideInInspector] public Coroutine TransitionCoroutine;
    }

    [System.Serializable]
    public class LegMaterialSettings
    {
        public Material targetMaterial;
        public string cutoffProperty = "Vector1_CFBBCBA";
        public float activeValue = 1f;
        public float inactiveValue = 10f;
        public float transitionSpeed = 1f;
        
        [HideInInspector] public Coroutine TransitionCoroutine;
    }

    [System.Serializable]
    public class HatMaterialSettings
    {
        public Material targetMaterial;
        public string cutoffProperty = "Vector1_CFBBCBA";
        public float activeValue = 3f;
        public float inactiveValue = 10f;
        public float transitionSpeed = 1f;
        
        [HideInInspector] public Coroutine TransitionCoroutine;
    }

    [Header("Material References")]
    [SerializeField] private HandMaterialSettings handMaterial;
    [SerializeField] private LegMaterialSettings legMaterial;
    [SerializeField] private HatMaterialSettings hatMaterial;

    [Header("Debug Options")]
    [SerializeField] private bool debugDisableAll = false;

    private PlayerControlInformationProcess _controlInfo;
    private bool _isCameraModeActive = false;
    private bool _isPlayerDead = false;
    private GameDataModel _data;

    private void Start()
    {
        _controlInfo = GetComponent<PlayerControlInformationProcess>();
        if (!_controlInfo)
        {
            Debug.LogError("PlayerControlInformationProcess not found!");
            return;
        }

        InitializeMaterial(handMaterial);
        InitializeMaterial(legMaterial);
        InitializeMaterial(hatMaterial);

        _controlInfo.onSwitchControlMode.AddListener(OnControlModeChanged);
        _controlInfo.onCameraControl.AddListener(OnCameraModeActivated);
        _controlInfo.onStopCameraControl.AddListener(OnCameraModeDeactivated);
        
        if (_data)
        {
            _data.OnIsPlayerDiedChangedEvent += PlayerDie;
        }

        UpdateMaterialStates();
    }

    private void OnDestroy()
    {
        if (!_controlInfo) return;
        
        _controlInfo.onSwitchControlMode.RemoveListener(OnControlModeChanged);
        _controlInfo.onCameraControl.RemoveListener(OnCameraModeActivated);
        _controlInfo.onStopCameraControl.RemoveListener(OnCameraModeDeactivated);
        
        if (_data)
        {
            _data.OnIsPlayerDiedChangedEvent -= PlayerDie;
        }
    }

    private void PlayerDie(bool isDead)
    {
        _isPlayerDead = isDead;
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
        if (!_isCameraModeActive)
        {
            UpdateMaterialStates();
        }
    }

    private void OnCameraModeActivated()
    {
        _isCameraModeActive = true;
        UpdateMaterialStates();
    }

    private void OnCameraModeDeactivated()
    {
        _isCameraModeActive = false;
        UpdateMaterialStates();
    }

    private void UpdateMaterialStates()
    {
        if (debugDisableAll || _isPlayerDead)
        {
            ResetAllMaterials();
            return;
        }

        if (_isCameraModeActive)
        {
            SetMaterialValue(handMaterial, handMaterial.inactiveValue);
            SetMaterialValue(legMaterial, legMaterial.inactiveValue);
            SetMaterialValue(hatMaterial, hatMaterial.activeValue);
        }
        else
        {
            switch (_controlInfo.mCurrentControlMode)
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
        if (!settings.targetMaterial || !settings.targetMaterial.HasProperty(settings.cutoffProperty))
            return;

        if (settings.TransitionCoroutine != null)
        {
            StopCoroutine(settings.TransitionCoroutine);
        }

        settings.TransitionCoroutine = StartCoroutine(TransitionMaterial(
            settings.targetMaterial, 
            settings.cutoffProperty, 
            settings.targetMaterial.GetFloat(settings.cutoffProperty), 
            targetValue, 
            settings.transitionSpeed
        ));
    }

    private void SetMaterialValue(LegMaterialSettings settings, float targetValue)
    {
        if (!settings.targetMaterial || !settings.targetMaterial.HasProperty(settings.cutoffProperty))
            return;

        if (settings.TransitionCoroutine != null)
        {
            StopCoroutine(settings.TransitionCoroutine);
        }

        settings.TransitionCoroutine = StartCoroutine(TransitionMaterial(
            settings.targetMaterial, 
            settings.cutoffProperty, 
            settings.targetMaterial.GetFloat(settings.cutoffProperty), 
            targetValue, 
            settings.transitionSpeed
        ));
    }

    private void SetMaterialValue(HatMaterialSettings settings, float targetValue)
    {
        if (!settings.targetMaterial || !settings.targetMaterial.HasProperty(settings.cutoffProperty))
            return;

        if (settings.TransitionCoroutine != null)
        {
            StopCoroutine(settings.TransitionCoroutine);
        }

        settings.TransitionCoroutine = StartCoroutine(TransitionMaterial(
            settings.targetMaterial, 
            settings.cutoffProperty, 
            settings.targetMaterial.GetFloat(settings.cutoffProperty), 
            targetValue, 
            settings.transitionSpeed
        ));
    }

    private IEnumerator TransitionMaterial(Material material, string property, float startValue, float targetValue, float speed)
    {
        var elapsedTime = 0f;
        var duration = 1f / Mathf.Max(0.01f, speed);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            var currentValue = Mathf.Lerp(startValue, targetValue, t);
            material.SetFloat(property, currentValue);
            yield return null;
        }

        material.SetFloat(property, targetValue);
    }

    private void ResetAllMaterials()
    {
        StopAllCoroutines();

        if (handMaterial.targetMaterial && handMaterial.targetMaterial.HasProperty(handMaterial.cutoffProperty))
            handMaterial.targetMaterial.SetFloat(handMaterial.cutoffProperty, handMaterial.inactiveValue);
        
        if (legMaterial.targetMaterial && legMaterial.targetMaterial.HasProperty(legMaterial.cutoffProperty))
            legMaterial.targetMaterial.SetFloat(legMaterial.cutoffProperty, legMaterial.inactiveValue);
        
        if (hatMaterial.targetMaterial && hatMaterial.targetMaterial.HasProperty(hatMaterial.cutoffProperty))
            hatMaterial.targetMaterial.SetFloat(hatMaterial.cutoffProperty, hatMaterial.inactiveValue);
    }
}