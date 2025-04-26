using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EventBus<T> where T : class
{
    // 存储所有订阅（带参数）
    private static readonly HashSet<Action<T>> Listeners = new();

    // 存储所有订阅（无参数）
    private static readonly HashSet<Action> ParameterlessListeners = new();

    // 存储对象与其订阅的委托（使用 WeakReference 避免内存泄漏）
    private static readonly Dictionary<WeakReference, List<object>> SubscriberToDelegatesMap = new();

    // 订阅带参数的事件
    public static void Subscribe(Action<T> listener, MonoBehaviour subscriber)
    {
        if (subscriber == null)
        {
            Debug.LogWarning("订阅者不能为 null！");
            return;
        }

        Listeners.Add(listener);
        RegisterSubscription(subscriber, listener);
    }

    // 订阅无参数的事件
    public static void Subscribe(Action listener, MonoBehaviour subscriber)
    {
        if (subscriber == null)
        {
            Debug.LogWarning("订阅者不能为 null！");
            return;
        }

        ParameterlessListeners.Add(listener);
        RegisterSubscription(subscriber, listener);
    }

    // 注册订阅关系
    private static void RegisterSubscription(MonoBehaviour subscriber, object listener)
    {
        CleanupDeadReferences(); // 先清理无效引用

        // 查找是否已存在该订阅者的记录
        var subscriberRef = (from kvp in SubscriberToDelegatesMap where (MonoBehaviour)kvp.Key.Target == subscriber select kvp.Key).FirstOrDefault();

        // 如果没有记录，则创建新记录
        if (subscriberRef == null)
        {
            subscriberRef = new WeakReference(subscriber);
            SubscriberToDelegatesMap[subscriberRef] = new List<object>();
        }

        // 添加订阅
        if (!SubscriberToDelegatesMap[subscriberRef].Contains(listener))
        {
            SubscriberToDelegatesMap[subscriberRef].Add(listener);
        }
    }

    // 清理无效引用（自动调用）
    private static void CleanupDeadReferences()
    {
        var deadRefs = (from kvp in SubscriberToDelegatesMap where !kvp.Key.IsAlive select kvp.Key).ToList();

        foreach (var deadRef in deadRefs)
        {
            SubscriberToDelegatesMap.Remove(deadRef);
        }
    }

    // 清理某个订阅者的所有订阅
    public static void UnsubscribeAll(MonoBehaviour subscriber)
    {
        if (subscriber == null) return;

        CleanupDeadReferences();

        var targetRef = (from kvp in SubscriberToDelegatesMap where (MonoBehaviour)kvp.Key.Target == subscriber select kvp.Key).FirstOrDefault();

        if (targetRef == null) return;
        // 移除所有订阅
        foreach (var listener in SubscriberToDelegatesMap[targetRef])
        {
            switch (listener)
            {
                case Action<T> paramAction:
                    Listeners.Remove(paramAction);
                    break;
                case Action paramlessAction:
                    ParameterlessListeners.Remove(paramlessAction);
                    break;
            }
        }

        SubscriberToDelegatesMap.Remove(targetRef);
    }

    // 发布事件（带参数）
    public static void Publish(T eventData = null)
    {
        CleanupDeadReferences(); // 先清理无效引用

        foreach (var listener in Listeners)
        {
            try { listener?.Invoke(eventData); }
            catch (Exception e) { Debug.LogError($"事件执行失败: {e}"); }
        }

        foreach (var listener in ParameterlessListeners)
        {
            try { listener?.Invoke(); }
            catch (Exception e) { Debug.LogError($"事件执行失败: {e}"); }
        }
    }

    // 清空所有订阅（场景切换时调用）
    public static void Clear()
    {
        Listeners.Clear();
        ParameterlessListeners.Clear();
        SubscriberToDelegatesMap.Clear();
    }
}