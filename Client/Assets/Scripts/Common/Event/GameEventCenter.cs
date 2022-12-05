
//////////////////////////////////////////////////////////////////////////
//
//   FileName : GameEventCenter.cs
//     Author : Chiyer
// CreateTime : 2014-04-28
//       Desc :
//
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEventCenter
{
    private class EventListenerSet
    {
        internal EventListener listener; 
    }

    private static readonly Dictionary<string, EventListenerSet> eventListeners;
    public delegate void EventListener(object argument);

    static GameEventCenter()
    {
        removes__ = new List<string>();
        eventListeners = new Dictionary<string, EventListenerSet>();
    }

    public static void Send(string _event, object argument = null)
    {
        EventListenerSet listenerSet;
        if (eventListeners.TryGetValue(_event, out listenerSet))
        {
            var listeners = listenerSet.listener.GetInvocationList();
            if (listeners.Length > 0)
            {
                for (var i = 0; i < listeners.Length; i++)
                {
                    var eventListener = listeners[i] as EventListener;
                    if (eventListener != null)
                    {
                        try
                        {
                            eventListener(argument);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(ex.ToString());
                        }
                    }
                }
            }
        }
    }

    public static void AddListener(string _event, EventListener _listener)
    {
        EventListenerSet listenerSet;
        if (!eventListeners.TryGetValue(_event, out listenerSet))
        {
            listenerSet = new EventListenerSet();
            eventListeners.Add(_event, listenerSet);
        }
        else
            listenerSet.listener -= _listener;
        listenerSet.listener += _listener;
    }

    public static void RemoveListener(string _event, EventListener _listener)
    {
        EventListenerSet listenerSet;
        if (eventListeners.TryGetValue(_event, out listenerSet))
            if ((listenerSet.listener -= _listener) == null)
                eventListeners.Remove(_event);
    }

    public static void RemoveListener(string _event)
    {
        eventListeners.Remove(_event);
    }

    static List<string> removes__;

    public static void RemoveListener(EventListener _listener)
    {
        removes__.Clear();
        var e = eventListeners.GetEnumerator();
        using (e)
        { 
            while (e.MoveNext())
                if ((e.Current.Value.listener -= _listener) == null)
                    removes__.Add(e.Current.Key);
        }
        var c = removes__.Count;
        for (var i = 0; i < c; ++i)
            eventListeners.Remove(removes__[i]);
    }

    public static void ClearListener()
    {
        eventListeners.Clear();
    }

    public static void RemoveListener(System.Object target)
    {
        removes__.Clear();
        using (var e = eventListeners.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var targetDelegates =
                    Array.FindAll(
                        e.Current.Value.listener.GetInvocationList(), _delgate => _delgate.Target == target);
                for (var j = 0; j < targetDelegates.Length; ++j)
                    e.Current.Value.listener -= (EventListener)targetDelegates[j];
                if (e.Current.Value.listener == null)
                    removes__.Add(e.Current.Key);
            }
            var c = removes__.Count;
            for (var i = 0; i < c; ++i)
                eventListeners.Remove(removes__[i]);
        }
    }
}