using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class XUI_CacheInstance
{
    private static readonly Dictionary<Type, List<XUI_CacheInstance>> CachePool;
    private static readonly Dictionary<Type, List<XUI_CacheInstance>> ActiveList;

    static XUI_CacheInstance()
    {
        CachePool = new Dictionary<Type, List<XUI_CacheInstance>>();
        ActiveList = new Dictionary<Type, List<XUI_CacheInstance>>();
    }

    public bool InCache;

    private static Transform _cacheRoot;

    private static Transform CacheRoot
    {
        get
        {
            if (_cacheRoot)
            {
                return _cacheRoot;
            }

            var factory = ObjectHelper.CreateGameObject("XUI_CacheInstance");
            factory.DontDestroyOnSceneChanged();
            _cacheRoot = factory.transform;
            factory.SetActiveSafe(false);
            return _cacheRoot;
        }
    }

    //每个类型都用字典存着，统一处理
    private static List<XUI_CacheInstance> GetCachePool(Type t)
    {
        if (!CachePool.TryGetValue(t, out var caches))
        {
            caches = new List<XUI_CacheInstance>();
            CachePool.Add(t, caches);
        }

        return caches;
    }

    public static List<XUI_CacheInstance> GetActiveInstances(Type t)
    {
        List<XUI_CacheInstance> caches;
        if (!ActiveList.TryGetValue(t, out caches))
        {
            caches = new List<XUI_CacheInstance>();
            ActiveList.Add(t, caches);
        }

        return caches;
    }

    protected static void Recycle(XUI_CacheInstance inst, Type t)
    {
        if (inst == null || inst.InCache)
        {
            return;
        }

        var actives = GetActiveInstances(t);
        actives.Remove(inst);

        var caches = GetCachePool(t);

        //缓存满了，直接destroy
        if (caches.Count >= inst.GetMaxCacheCount())
        {
            XUI_Manager.Close(inst.GetIXUI());
        }
        else
        {
            inst.InCache = true;
            inst.GetIXUI().GetGameObject().SetActiveSafe(false);
            inst.OnRecycle();

            if (inst.GetIXUI().GetGameObject().transform.parent != CacheRoot)
            {
                inst.InCache = true;
                inst.GetIXUI().GetGameObject().transform.SetParent(CacheRoot, false);
            }

            if (!caches.Contains(inst))
            {
                caches.Add(inst);
            }
        }
    }

    public static T CreateInstFromPool<T>(Transform parent = null) where T : XUI_CacheInstance, IXUI
    {
        var caches = GetCachePool(typeof(T));
        var actives = GetActiveInstances(typeof(T));
        //终于，原来在这里加载了预制体
        if (caches.Count == 0)
        {
            CacheNew<T>();
        }

        return Create<T>(caches, actives, parent);
    }

    private static T Create<T>(List<XUI_CacheInstance> caches, List<XUI_CacheInstance> actives, Transform parent = null) where T : XUI_CacheInstance, IXUI
    {
        XUI_CacheInstance inst = null;
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
            inst.InCache = false;
            inst.GetIXUI().GetGameObject().SetActiveSafe(true);
            caches.RemoveAt(0);
        }


        if (inst == null)
        {
            Debug.LogError("GetInstFromPool == null, 池内没有可用的UIInst，Type:" + typeof(T));
        }
        else
        {
            actives.Add(inst);

            //parent
            //如果有父母则认，没有则放到Prefer对应的层,并且显示在最前面
            if (parent == null)
            {
                var t = XUI_Manager.GetCanvas(inst.GetIXUI().GetLayer());
                inst.GetIXUI().GetGameObject().transform.SetParent(t, false);
                inst.GetIXUI().GetGameObject().transform.SetAsLastSibling();
            }
            else
            {
                inst.GetIXUI().GetGameObject().transform.SetParent(parent, false);
                inst.GetIXUI().GetGameObject().transform.SetAsLastSibling();
            }
        }

        return inst as T;
    }

    private static void CacheNew<T>() where T : XUI_CacheInstance, IXUI
    {
        var caches = GetCachePool(typeof(T));

        if (Common.CreateInstance(typeof(T)) is T view)
        {
            XUI_Manager.Show(view, view.GetLayer());
            view.InCache = true;
            view.GetGameObject().SetActiveSafe(false);
            caches.Add(view);
        }
    }

    public static void ClearAll()
    {
        foreach (var kv in ActiveList)
        {
            for (var i = 0; i < kv.Value.Count; i++)
            {
                if (kv.Value[i].GetIXUI().GetGameObject() != null)
                {
                    Object.DestroyImmediate(kv.Value[i].GetIXUI().GetGameObject());
                }
            }
        }

        foreach (var kv in CachePool)
        {
            for (var i = 0; i < kv.Value.Count; i++)
            {
                if (kv.Value[i].GetIXUI().GetGameObject() != null)
                {
                    Object.DestroyImmediate(kv.Value[i].GetIXUI().GetGameObject());
                }
            }
        }

        ActiveList.Clear();
        CachePool.Clear();
    }

    protected abstract int GetMaxCacheCount();
    protected abstract IXUI GetIXUI();


    public void DoRecycle()
    {
        //已经被销毁了
        if (GetIXUI().GetGameObject() == null)
        {
            return;
        }

        Recycle(this, GetType());
    }

    protected virtual void OnRecycle()
    {
    }
}