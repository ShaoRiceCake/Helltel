using UnityEngine;
using TMPro;

public class GoldGetShowTool : MonoBehaviour
{
    //[Header("UI Settings")]
    //public TMP_Text counterText;
    //public string prefixText = "获取金币： "; 

    private FluidParticleCounter _particleCounter;
    //private int _destroyedParticleCount;

    private void Awake()
    {
        _particleCounter = GetComponent<FluidParticleCounter>();
        if (_particleCounter == null)
        {
            Debug.LogError("FluidParticleCounter component not found!");
            return;
        }

        _particleCounter.OnParticleDestroyed += OnParticleDestroyed;
    }

    private void Start()
    {
        //UpdateCounterText();
    }

    private void OnDestroy()
    {
        if (_particleCounter != null)
        {
            _particleCounter.OnParticleDestroyed -= OnParticleDestroyed;
        }
    }

    private void OnParticleDestroyed(int particleIndex)
    {
        //_destroyedParticleCount++;
        GameController.Instance.AddPerformance(1);
        //UpdateCounterText();注释此行，因为Performance改变后会发送事件使UI会同步更新
    }

    // private void UpdateCounterText()
    // {
    //     if (counterText != null)
    //     {
    //         counterText.text = prefixText + _destroyedParticleCount.ToString();
    //     }
    // }

    // public void ResetCounter()
    // {
    //     _destroyedParticleCount = 0;
    //     //UpdateCounterText();
    // }
}
