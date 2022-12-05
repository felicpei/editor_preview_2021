
using UnityEngine;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;
using DG.Tweening;

public static class XUI_Manager
{
    private static GameObject _root;
    private static XUI_Bind _bind;
    
    private static readonly Dictionary<XUI_MissionLayer, Transform> MissionCanvasList = new();

    private static PointerEventData _pointerEventData;
    private static List<RaycastResult> _raycastResult;
    
    private static readonly Dictionary<uint, IXUI> UIInstances = new();
    private static readonly List<uint> WantToRemove = new();
    private static readonly List<IXUI> WantToAdd = new();
    private static readonly Dictionary<XUI_Layer, RectTransform> CanvasParentList = new();
    private static readonly Dictionary<XUI_Layer, Canvas> CanvasList = new();
    
    
    public static void Init()
    {
        _bind = Object.FindObjectOfType<XUI_Bind>();
        _root = _bind.gameObject;
        Object.DontDestroyOnLoad(_root);

        CanvasParentList[XUI_Layer.Splash] = _bind.Splash;
        CanvasParentList[XUI_Layer.Main] = _bind.MainCity;
        CanvasParentList[XUI_Layer.View3D] = _bind.View;
        CanvasParentList[XUI_Layer.Loding] = _bind.Loding;
        CanvasParentList[XUI_Layer.Tips] = _bind.Tips;

        CanvasList[XUI_Layer.Splash] = _bind.Splash.GetComponent<Canvas>();
        CanvasList[XUI_Layer.Main] = _bind.MainCity.GetComponent<Canvas>();
        CanvasList[XUI_Layer.View3D] = _bind.View.GetComponent<Canvas>();
        CanvasList[XUI_Layer.Loding] = _bind.Loding.GetComponent<Canvas>();
        CanvasList[XUI_Layer.Tips] = _bind.Tips.GetComponent<Canvas>();

        //注册UI
        _pointerEventData = new PointerEventData(EventSystem.current);
        _raycastResult = new List<RaycastResult>();

        ScreenScaleHelper.Init();
    }

    public static List<RaycastResult> GetRayCastResult(Vector2 pos)
    {
        _raycastResult.Clear();
        _pointerEventData.position = pos;
        EventSystem.current.RaycastAll(_pointerEventData, _raycastResult);
        return _raycastResult;
    }


    public static RectTransform GetCanvas(XUI_Layer layer)
    {
        return CanvasParentList[layer];
    }

    private static Canvas GetCanvasByLayer(XUI_Layer layer)
    {
        return CanvasList[layer];
    }

    public static Transform GetMissionCanvas(XUI_MissionLayer l)
    {
        if (!MissionCanvasList.TryGetValue(l, out var parent))
        {
            var gamObj = new GameObject(l.ToString());
            gamObj.transform.SetParent(_bind.MissionCanvas.transform, false);
            var canvas = gamObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = CameraHelper.Camera;
            parent = gamObj.transform;
            MissionCanvasList.Add(l, parent);
        }
        return parent;
    }

    public static Vector2 OriginCanvasSize;
    public static void RefreshCanvasSize(float widthRadio, float heightRadio, float startXPercent, float startYPercent)
    {
      
        /*SetDeltaInfo(CanvasList[XUI_Layer.Main],  widthRadio, heightRadio, startXPercent, startYPercent);
        SetDeltaInfo(CanvasList[XUI_Layer.View3D],  widthRadio, heightRadio, startXPercent, startYPercent);
        SetDeltaInfo(CanvasList[XUI_Layer.Tips],  widthRadio, heightRadio, startXPercent, startYPercent);
        SetDeltaInfo(CanvasList[XUI_Layer.Loding],  1, 1, 0, 0);
        SetDeltaInfo(CanvasList[XUI_Layer.Splash],  1, 1, 0, 0);*/
    }
    
    
    public static void DestroySplashCanvas()
    {
        var splash = GetCanvas(XUI_Layer.Splash);
        if (splash != null)
        {
            splash.gameObject.DestroyAllChild();
        }
    }


    private static void ClearAllUi()
    {
        //清空层里面的所有物体
        if (_bind != null && CanvasList != null)
        {
            var loop = CanvasList.GetEnumerator();
            using (loop)
            {
                while (loop.MoveNext())
                {
                    var key = loop.Current.Key;
                    if (key == XUI_Layer.Loding)
                    {
                        continue;
                    }
                    
                    if (loop.Current.Value != null)
                    {
                        loop.Current.Value.gameObject.DestroyAllChild();
                    }
                }
            }
        }

        //清空 Mission canvas
        var enumerator = MissionCanvasList.GetEnumerator();
        using (enumerator)
        {
            while (enumerator.MoveNext())
            {
                var missionCanvas = enumerator.Current.Value;
                if (missionCanvas != null)
                {
                    missionCanvas.gameObject.DestroyAllChild();
                }
            }
        }
        
        TryRemoveUnuse();
    }

    public static void ShowMissionUgui(Transform t, XUI_MissionLayer layer)
    {
        var parent = GetMissionCanvas(layer);
        if (parent != null)
        {
            t.SetParent(parent, false);
        }
    }
    
    private static uint _runtimeIdSequence;

    public static uint GenRunTimeId()
    {
        return ++_runtimeIdSequence;
    }
    
    //显示界面
    public static void Show(IXUI view, XUI_Layer layer, Action callBack = null) 
    {
        if (UIInstances.ContainsKey(view.GetUniqueId()))
        {
            Debug.LogError("XUI_Manager Show Error, 已经存在的UniqueId:" + view.GetPrefabPath());
            return;
        }

        XResource.LoadGameObject(view.GetPrefabPath(), uiObject =>
        {
            ShowUiView(uiObject, view, layer);
            callBack?.Invoke();
        });
    }
    
    private static void ShowUiView(GameObject uiObject, IXUI view, XUI_Layer layerName)
    {
        var layer = Enum.GetName(typeof(XUI_Layer), layerName);

        var parent = CanvasParentList[layerName];
        if (parent == null)
        {
            Object.Destroy(uiObject);
            Debug.LogError("ShowUIView 层级错误，使用了存在不存在的层：" + layer);
            return;
        }

        uiObject.SetActiveSafe(true); 
        view.BeforeInit(uiObject, layerName);
        
        //挂个通用脚本
        var commonMono = uiObject.AddComponentIfNotExists<XUI_CommonMono>();
        commonMono.Init(view);
        
        uiObject.transform.SetParent(parent, false);
        uiObject.transform.SetAsLastSibling();
        view.OnShow();
        
        WantToAdd.Add(view);
    }

    public static void SetToCanvas(Transform transform, XUI_Layer layerName)
    {
        var layer = Enum.GetName(typeof(XUI_Layer), layerName);

        var parent = CanvasParentList[layerName];
        if (parent == null)
        {
            Debug.LogError("SetToCanvas 层级错误，使用了存在不存在的层：" + layer);
            return;
        }
        transform.SetParent(parent, false);
        transform.SetAsLastSibling();
        transform.gameObject.SetActiveSafe(true);
    }

    
    //显示UI特效
    public static void ShowEffect(GameObject effect, XUI_Layer layerName = XUI_Layer.Tips, int sortingOrder = 5)
    {
        var layer = Enum.GetName(typeof(XUI_Layer), layerName);
        Transform parent = CanvasParentList[layerName];
        if (parent == null)
        {
            Debug.LogError("ShowUIView 层级错误，使用了存在不存在的层：" + layer);
            return;
        }

        var setting = effect.AddComponentIfNotExists<XUI_SetMeshRenderOrder>();
        var sortingLayer = Layers.GetSortingLayer(layerName);
        setting.SoringLayer = sortingLayer;
        setting.sortingOrder = sortingOrder;
        setting.Sort();

        var particles = effect.GetComponentsInChildren<ParticleSystemRenderer>();
        for (var i = 0; i < particles.Length; i++)
        {
            var particle = particles[i];
            particle.sortingLayerID =  SortingLayer.NameToID(sortingLayer.ToString()); 
            particle.sortingOrder = sortingOrder;
        }

        effect.transform.SetLayer(Layers.UI);
        effect.gameObject.transform.SetParent(parent, false);
    }

    public static void ShowFullScreenEffect(GameObject effect, XUI_Layer layerName = XUI_Layer.Tips)
    {
        ShowEffect(effect, layerName);
        var canvas = GetCanvasByLayer(layerName);
        var rect = canvas.GetComponent<RectTransform>();
        var sizeDelta = rect.sizeDelta;
        effect.transform.localScale = new Vector3(sizeDelta.x, sizeDelta.y, 1f);
    }

    //关闭窗口
    public static void Close(this IXUI instance)
    {
        if (instance == null)
        {
            Debug.LogError("XUI_MANAGER CLOSE ERROR, instance IS NULL");
            return;
        }

        if (UIInstances.TryGetValue(instance.GetUniqueId(), out _)) 
        {
            instance.GetGameObject().GetComponent<XUI_CommonMono>().OnDestroy();
            Object.Destroy(instance.GetGameObject());
        }
        else
        {
            Debug.LogWarning("Close UI 错误， _uiInstances 不存在的ID:" + instance.GetUniqueId() + " name:" + instance.GetPrefabPath() +"(是不是有引用没清?)");
        }
    }

    
    public static void OnUIDestroy(IXUI instance)
    {
        WantToRemove.Add(instance.GetUniqueId());
    }

    public static void RemoveAllChild(this Transform instance)
    {
        if (instance == null)
        {
            return;
        }
        
        for (var i = 0; i < instance.childCount; i++)
        {
            var item = instance.GetChild(i);
            Object.Destroy(item.gameObject);
        }
        instance.DetachChildren();
    }

    public static void Clear()
    {
        DOTween.KillAll();
        DOTween.Clear(true);
        XUI_CacheMissionUi.ClearAll();
        XUI_CacheInstance.ClearAll();
        ClearAllUi();
        MissionCanvasList.Clear();
        TryRemoveUnuse();
    }

    public static void OnApplicationQuit()
    {
        XUI_CacheMissionUi.ClearAll();
        XUI_CacheInstance.ClearAll();
    }

    private static void TryRemoveUnuse()
    {
        for (var i = 0; i < WantToRemove.Count; i++)
        {
            UIInstances.Remove(WantToRemove[i]);
        }
        WantToRemove.Clear();
    }

    private static void TryAdd()
    {
        for (var i = 0; i < WantToAdd.Count; i++)
        {
            UIInstances.Add(WantToAdd[i].GetUniqueId(), WantToAdd[i]);
        }
        WantToAdd.Clear();
    }
    
    public static void OnUpdate()
    {
        //try add
        TryAdd();
        
        //Update 之前，把需要remove的先remove
        TryRemoveUnuse();

        foreach (var kv in UIInstances)
        {
            if (kv.Value.GetGameObject().activeSelf)
            {
                kv.Value.Update();
            }
        }    
    }
    
    public static void DoDestroy()
    {
        Clear();
        if (_bind != null && _bind.gameObject != null) 
        {
            Object.Destroy(_bind.gameObject);
        }
    }
}
