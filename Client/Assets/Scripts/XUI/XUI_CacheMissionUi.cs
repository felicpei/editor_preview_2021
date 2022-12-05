
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class XUI_CacheMissionUi : XUI_GameObject
{
    private static readonly Dictionary<Type, List<XUI_CacheMissionUi>> CachePool;
    private static readonly Dictionary<Type, List<XUI_CacheMissionUi>> ActiveList;

    static XUI_CacheMissionUi()
    {
        CachePool = new Dictionary<Type, List<XUI_CacheMissionUi>>();
        ActiveList = new Dictionary<Type, List<XUI_CacheMissionUi>>();
    }

    public bool InCache;

    public static List<XUI_CacheMissionUi> GetCachePool(Type t)
    {
        List<XUI_CacheMissionUi> caches;
        if (!CachePool.TryGetValue(t, out caches))
        {
            caches = new List<XUI_CacheMissionUi>();
            CachePool.Add(t, caches);
        }

        return caches;
    }

    public static List<XUI_CacheMissionUi> GetActiveInstances(Type t)
    {
        List<XUI_CacheMissionUi> caches;
        if (!ActiveList.TryGetValue(t, out caches))
        {
            caches = new List<XUI_CacheMissionUi>();
            ActiveList.Add(t, caches);
        }

        return caches;
    }

    public static void Recycle(XUI_CacheMissionUi inst, Type t)
    {
        if (inst == null) return;

        var actives = GetActiveInstances(t);
        actives.Remove(inst);

        var caches = GetCachePool(t);
        inst.InCache = true;
        inst.SetActiveByCanvasGroup(false);
        try
        {
            inst.OnRecycle();
        }
        catch (Exception ex)
        {
            Debug.LogError("Recycle error " + inst + " " + ex);
        }

        if (!caches.Contains(inst))
        {
            caches.Add(inst);
        }
    }

    public static void CreateInstFromPool<T>(XUI_MissionLayer layer, string url, int maxCount, Action<XUI_CacheMissionUi> callBack) where T : XUI_CacheMissionUi
    {
        var caches = GetCachePool(typeof(T));
        var actives = GetActiveInstances(typeof(T));
        if (caches.Count == 0 && actives.Count < maxCount)
        {
            CacheNew<T>(url, () => { Create<T>(layer, caches, actives, callBack); });
        }
        else
        {
            Create<T>(layer, caches, actives, callBack);
        }
    }


    public static void Create<T>(XUI_MissionLayer layer, List<XUI_CacheMissionUi> caches, List<XUI_CacheMissionUi> actives, Action<XUI_CacheMissionUi> callBack) where T : XUI_CacheMissionUi
    {
        XUI_CacheMissionUi inst = null;
        if (caches.Count == 0)
        {
            if (actives.Count > 0)
            {
                Recycle(actives[0], actives[0].GetType());
            }
        }

        if (caches.Count > 0)
        {
            inst = caches[0];
            if (inst != null)
            {
                inst.InCache = false;
                inst.SetActiveByCanvasGroup(true);
            }
            else
            {
                Debug.LogError("XUI_CacheMissionUI Cache0 is null:" + typeof(T));
            }

            caches.RemoveAt(0);
        }

        if (inst == null)
        {
            Debug.LogError("GetInstFromPool == null, 池内没有可用的UIInst，Type:" + typeof(T));
            callBack(null);
            return;
        }
        else
        {
            actives.Add(inst);
        }

        inst.SetLayer(layer);
        callBack(inst);
    }

    public void SetLayer(XUI_MissionLayer layer)
    {
        var parent = XUI_Manager.GetMissionCanvas(layer);
        if (parent != transform.parent)
        {
            transform.SetParent(parent, false);
        }
    }

    public static void CacheNew<T>(string url, Action callBack) where T : XUI_CacheMissionUi
    {
        var caches = GetCachePool(typeof(T));
        XResource.LoadGameObject(url, gameObj =>
        {
            var t = gameObj.AddComponentIfNotExists<T>();
            t.InCache = true;
            t.SetActiveByCanvasGroup(false);
            caches.Add(t);
            callBack();
        });
    }

    public static void ClearAll()
    {
        foreach (var kv in ActiveList)
        {
            for (var i = 0; i < kv.Value.Count; i++)
            {
                if (kv.Value[i].gameObject != null)
                {
                    DestroyImmediate(kv.Value[i].gameObject);
                }
            }
        }

        foreach (var kv in CachePool)
        {
            for (var i = 0; i < kv.Value.Count; i++)
            {
                if (kv.Value[i].gameObject != null)
                {
                    DestroyImmediate(kv.Value[i].gameObject);
                }
            }
        }

        ActiveList.Clear();
        CachePool.Clear();
    }

    protected virtual void OnRecycle()
    {
    }
}