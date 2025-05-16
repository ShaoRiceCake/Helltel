using UnityEngine;

// 消息类型枚举
public enum UIMessageType
{
    Info,    // 普通提示
    Warning  // 警告
}

// 消息事件数据类
public class UIMessageEvent
{
    public string Content { get; }      // 消息内容
    public float Duration { get; }      // 显示时长（秒）
    public UIMessageType Type { get; }    // 消息类型

    public UIMessageEvent(string content, float duration = 3f, UIMessageType type = UIMessageType.Info)
    {
        Content = content;
        Duration = duration;
        Type = type;
    }
}