using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-100)]
public class EventBus : MonoBehaviour
{
    private Dictionary<Type, List<Delegate>> subscribers = new();

    public void Subscribe<T>(Action<T> eventHandler) where T : struct
    {
        Type eventType = typeof(T);
        if (!subscribers.ContainsKey(eventType))
            subscribers[eventType] = new List<Delegate>();

        subscribers[eventType].Add(eventHandler);
    }

    public void Unsubscribe<T>(Action<T> eventHandler) where T : struct
    {
        Type eventType = typeof(T);
        if (!subscribers.TryGetValue(eventType, out var handlers))
            return;

        handlers.Remove(eventHandler);
    }

    public void Publish<T>(T gameEvent) where T : struct
    {
        Type eventType = typeof(T);
        if (!subscribers.TryGetValue(eventType, out var handlers))
            return;
        foreach (var handler in handlers.ToArray())
        {
            ((Action<T>)handler)?.Invoke(gameEvent);
        }
    }
}
