using UnityEngine;

public class PlayerTriggerDetector : MonoBehaviour
{
    private bool _isNotice;
    private void OnTriggerEnter(Collider other)
    {
        if (_isNotice) return;
        if (!other.CompareTag("Player")) return;
        EventBus<UIMessageEvent>.Publish(new UIMessageEvent("海绵可以擦除污渍，记得带上",3f));
        _isNotice = true;
    }
    
}