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
        public float flashSpeed = 2f; // Speed of the flashing effect
        
        [HideInInspector] public Coroutine TransitionCoroutine;
        [HideInInspector] public Coroutine FlashCoroutine;
    }

    [System.Serializable]
    public class LegMaterialSettings
    {
        public Material targetMaterial;
        public string cutoffProperty = "Vector1_CFBBCBA";
        public float activeValue = 1f;
        public float inactiveValue = 10f;
        public float transitionSpeed = 1f;
        public float flashSpeed = 2f; // Speed of the flashing effect
        
        [HideInInspector] public Coroutine TransitionCoroutine;
        [HideInInspector] public Coroutine FlashCoroutine;
    }

    [Header("Material References")]
    [SerializeField] private HandMaterialSettings handMaterial;
    [SerializeField] private LegMaterialSettings legMaterial;

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
        
        // Stop all coroutines
        StopAllCoroutines();
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
            // Camera mode - set both to inactive
            StopFlashing(handMaterial);
            StopFlashing(legMaterial);
            SetMaterialValue(handMaterial, handMaterial.inactiveValue);
            SetMaterialValue(legMaterial, legMaterial.inactiveValue);
        }
        else
        {
            switch (_controlInfo.mCurrentControlMode)
            {
                case PlayerControlInformationProcess.ControlMode.HandControl:
                    StopFlashing(legMaterial);
                    SetMaterialValue(legMaterial, legMaterial.inactiveValue);
                    StartFlashing(handMaterial);
                    break;
                
                case PlayerControlInformationProcess.ControlMode.LegControl:
                    StopFlashing(handMaterial);
                    SetMaterialValue(handMaterial, handMaterial.inactiveValue);
                    StartFlashing(legMaterial);
                    break;
            }
        }
    }

    private void StartFlashing(HandMaterialSettings settings)
    {
        if (!settings.targetMaterial || !settings.targetMaterial.HasProperty(settings.cutoffProperty))
            return;

        // Stop any existing transitions or flashes
        if (settings.TransitionCoroutine != null)
        {
            StopCoroutine(settings.TransitionCoroutine);
        }
        if (settings.FlashCoroutine != null)
        {
            StopCoroutine(settings.FlashCoroutine);
        }

        settings.FlashCoroutine = StartCoroutine(FlashMaterial(
            settings.targetMaterial, 
            settings.cutoffProperty, 
            settings.inactiveValue, 
            settings.activeValue, 
            settings.flashSpeed
        ));
    }

    private void StartFlashing(LegMaterialSettings settings)
    {
        if (!settings.targetMaterial || !settings.targetMaterial.HasProperty(settings.cutoffProperty))
            return;

        // Stop any existing transitions or flashes
        if (settings.TransitionCoroutine != null)
        {
            StopCoroutine(settings.TransitionCoroutine);
        }
        if (settings.FlashCoroutine != null)
        {
            StopCoroutine(settings.FlashCoroutine);
        }

        settings.FlashCoroutine = StartCoroutine(FlashMaterial(
            settings.targetMaterial, 
            settings.cutoffProperty, 
            settings.inactiveValue, 
            settings.activeValue, 
            settings.flashSpeed
        ));
    }

    private void StopFlashing(HandMaterialSettings settings)
    {
        if (settings.FlashCoroutine != null)
        {
            StopCoroutine(settings.FlashCoroutine);
            settings.FlashCoroutine = null;
        }
    }

    private void StopFlashing(LegMaterialSettings settings)
    {
        if (settings.FlashCoroutine != null)
        {
            StopCoroutine(settings.FlashCoroutine);
            settings.FlashCoroutine = null;
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

    private IEnumerator FlashMaterial(Material material, string property, float minValue, float maxValue, float speed)
    {
        while (true)
        {
            // Ping-pong between min and max values
            float t = Mathf.PingPong(Time.time * speed, 1f);
            float currentValue = Mathf.Lerp(minValue, maxValue, t);
            material.SetFloat(property, currentValue);
            yield return null;
        }
    }

    private void ResetAllMaterials()
    {
        StopAllCoroutines();

        if (handMaterial.targetMaterial && handMaterial.targetMaterial.HasProperty(handMaterial.cutoffProperty))
            handMaterial.targetMaterial.SetFloat(handMaterial.cutoffProperty, handMaterial.inactiveValue);
        
        if (legMaterial.targetMaterial && legMaterial.targetMaterial.HasProperty(legMaterial.cutoffProperty))
            legMaterial.targetMaterial.SetFloat(legMaterial.cutoffProperty, legMaterial.inactiveValue);
    }
}