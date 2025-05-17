using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MessageDisplay : MonoBehaviour
{
    // 外部依赖的TMP组件
    [SerializeField] private TMP_Text messageText;
    
    // 消息样式配置
    [Header("Style Settings")]
    [SerializeField] private Color infoColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    
    // 当前显示的消息（用于去重）
    private string _currentMessage;
    private Coroutine _hideCoroutine;

    private void Awake()
    {
        messageText.text = string.Empty;
        
        // 订阅消息事件
        EventBus<UIMessageEvent>.Subscribe(OnMessageReceived, this);
    }

    private void OnDestroy()
    {
        // 取消订阅
        EventBus<UIMessageEvent>.UnsubscribeAll(this);
    }

    // 消息处理逻辑
    private void OnMessageReceived(UIMessageEvent message)
    {
        // 如果是相同消息且正在显示，则忽略
        if (message.Content == _currentMessage && messageText.text != string.Empty)
        {
            return;
        }

        // 停止之前的隐藏协程（如果有）
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
        }

        // 更新UI
        _currentMessage = message.Content;
        messageText.text = message.Content;
        messageText.color = message.Type == UIMessageType.Info ? infoColor : warningColor;
        
        if(message.Type == UIMessageType.Warning)
            PlaySound();

        _hideCoroutine = StartCoroutine(HideAfterDelay(message.Duration));
    }

    private System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        messageText.text = string.Empty;
        _currentMessage = string.Empty;
    }

    private static void PlaySound()
    {
        AudioManager.Instance.Play("消息警告");
    }
}