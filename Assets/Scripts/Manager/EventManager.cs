using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

    public static void AddListener(string eventName, Action handler)
    {
        OnlistenerAdding(eventName, handler);
        eventTable[eventName] = (Action)eventTable[eventName] + handler;
    }

    public static void AddListener<T>(string eventName, Action<T> handler)
    {
        OnlistenerAdding(eventName, handler);
        eventTable[eventName] = (Action<T>)eventTable[eventName] + handler;
    }

    public static void AddListener<T1, T2>(string eventName, Action<T1, T2> handler)
    {
        OnlistenerAdding(eventName, handler);
        eventTable[eventName] = (Action<T1, T2>)eventTable[eventName] + handler;
    }

    public static void RemoveListener(string eventName, Action handler)
    {
        OnlistenerRemoving(eventName, handler);
        eventTable[eventName] = (Action)eventTable[eventName] - handler;
        OnlistenerRemoved(eventName);
    }

    public static void RemoveListener<T>(string eventName, Action<T> handler)
    {
        OnlistenerRemoving(eventName, handler);
        eventTable[eventName] = (Action<T>)eventTable[eventName] - handler;
        OnlistenerRemoved(eventName);
    }

    public static void RemoveListener<T1, T2>(string eventName, Action<T1, T2> handler)
    {
        OnlistenerRemoving(eventName, handler);
        eventTable[eventName] = (Action<T1, T2>)eventTable[eventName] - handler;
        OnlistenerRemoved(eventName);
    }

    public static void Broadcast(string eventName)
    {
        if (eventTable.TryGetValue(eventName, out Delegate d))
        {
            if (d is Action callback) callback.Invoke();
        }
    }

    public static void Broadcast<T>(string eventName, T arg1)
    {
        if (eventTable.TryGetValue(eventName, out Delegate d))
        {
            if (d is Action<T> callback) callback.Invoke(arg1);
        }
    }
    public static void Broadcast<T1, T2>(string eventName, T1 arg1, T2 arg2)
    {
        if (eventTable.TryGetValue(eventName, out Delegate d))
        {
            if (d is Action<T1, T2> callback) callback.Invoke(arg1, arg2);
        }
    }

    private static void OnlistenerAdding(string eventName, Delegate handler)
    {
        if (!eventTable.ContainsKey(eventName))
        {
            eventTable.Add(eventName, null);
        }

        if (eventTable[eventName] != null && eventTable[eventName].GetType() != handler.GetType())
        {
            throw new Exception(string.Format("尝试为事件{0}添加不同类型的委托，当前类型为{1}，要添加的类型为{2}", eventName, eventTable[eventName].GetType().Name, handler.GetType().Name));
        }
    }

    private static void OnlistenerRemoving(string eventName, Delegate handler)
    {
        if (eventTable.ContainsKey(eventName))
        {
            if (eventTable[eventName] == null)
            {

            }
            else if (eventTable[eventName].GetType() != handler.GetType())
            {

            }
        }
    }

    private static void OnlistenerRemoved(string eventName)
    {
        if (eventTable[eventName] == null)
        {
            eventTable.Remove(eventName);
        }
    }

    public static void ClearAll()
    {
        eventTable.Clear();
    }
}
