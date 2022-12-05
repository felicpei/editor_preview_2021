using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

// Class takes care of loading assetBundle and its dependencies automatically, loading variants automatically.
public class XAssetBundle
{
    // Loaded assetBundle contains the references count which can be used to unload dependent assetBundles automatically.
    private class LoadedAssetBundle
    {
        public readonly AssetBundle AssetBundle;
        public int ReferencedCount;

        public LoadedAssetBundle(AssetBundle assetBundle)
        {
            AssetBundle = assetBundle;
            ReferencedCount = 1;
        }
    }

   
    private static bool _inited;
    private static AssetBundleManifest _manifest;
    private static readonly Dictionary<string, LoadedAssetBundle> LoadedAssetBundles = new();
    private static readonly Dictionary<string, bool> LoadingAssetBundles = new();
    private static readonly Dictionary<string, float> LoadingAssetBundlesTime = new();
    private static readonly Dictionary<string, string[]> Dependencies = new();

    //初始化assetbundle
    public static IEnumerator Initialize(Action<string, float> onProgress)
    {
        if (_inited)
        {
            yield break;
        }

        if (XPlatform.Platform == XPlatform.EPlatform.UnityEditor)
        {
            _inited = true;
            yield break;
        }
        
        if (_manifest == null)
        {
            LoadedAssetBundles.Clear();

            //load Manifest
            var maniAsync = new AsyncResource();
            yield return RequestAssetBundle("StreamingAssets", maniAsync, onProgress);

            _manifest = maniAsync.AssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            _inited = true;
        }
    }


    // Load AssetBundle and its dependencies.
    public static IEnumerator LoadAssetBundle(string assetBundleName, Action<string, float> onProgress)
    {
        if (_manifest == null)
        {
            Debug.LogError("AssetBundle未初始化，Manifest == null 是否未调用 AssetBundleManager.Initialize()");
            yield break;
        }
        //Debug.Log("LoadAssetBundle assetbundle:" + assetBundleName);
        yield return LoadAssetBundleInternal(assetBundleName, onProgress);
    }
    
    // Get loaded AssetBundle, only return vaild object when all the dependencies are downloaded successfully.
    private static LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName)
    {
        LoadedAssetBundles.TryGetValue(assetBundleName, out var bundle);
        if (bundle == null)
        {
            return null;
        }

        // No dependencies are recorded, only the bundle itself is required.
        if (!Dependencies.TryGetValue(assetBundleName, out var dependencies))
        {
            return bundle;
        }

        // Make sure all dependencies are loaded
        foreach (var dependency in dependencies)
        {
            // Wait all the dependent assetBundles being loaded.
            LoadedAssetBundles.TryGetValue(dependency, out var dependentBundle);
            if (dependentBundle == null)
            {
                Debug.LogError("Get LoadedAssetBundles Dependencies Error, dependency is null:" + dependency);
                return null;
            }
        }

        return bundle;
    }

    private static IEnumerator LoadAssetBundleInternal(string assetBundleName, Action<string, float> onProgress)
    {
        //同名的bundle正在加载，则等待上一个完成
        if (LoadingAssetBundles.TryGetValue(assetBundleName, out var inLoading) && inLoading)
        {
            yield return new WaitUntil(() => CheckAssetLoaded(assetBundleName));
        }
        
        //新加载一个
        LoadingAssetBundles[assetBundleName] = true;
        LoadingAssetBundlesTime[assetBundleName] = Time.time;
        
        //如果依赖没有被加载过，加载一次这个bundle的依赖
        if (!Dependencies.ContainsKey(assetBundleName))
        {
            yield return LoadDependencies(assetBundleName, onProgress);    
        }
        
        //已经加载过的bundle，直接取缓存（此时不用加载依赖，第一次的时候已经加载）
        LoadedAssetBundles.TryGetValue(assetBundleName, out var bundle);
        if (bundle != null)
        {
            bundle.ReferencedCount++;
            LoadingAssetBundles[assetBundleName] = false;
            yield break;
        }
      
        //加载bundle，放入缓存
        var async = new AsyncResource();
        yield return RequestAssetBundle(assetBundleName, async, onProgress);
        LoadedAssetBundles[assetBundleName] = new LoadedAssetBundle(async.AssetBundle);
        LoadingAssetBundles[assetBundleName] = false;
    }

    private static bool CheckAssetLoaded(string assetBundleName)
    {
        LoadingAssetBundles.TryGetValue(assetBundleName, out var inLoading);
        if (inLoading == false)
        {
            return true;
        }
        
        if (Time.time - LoadingAssetBundlesTime[assetBundleName] > 10)
        {
            Debug.LogError($"加载assetbundle: {assetBundleName} 卡住了, 检查是否出现的循环引用");
        }


        //看下有没有被依赖加载过
        if (LoadedAssetBundles.TryGetValue(assetBundleName, out _))
        {
            LoadingAssetBundles[assetBundleName] = false;
            return true;
        }
        return false;
    }
    

    public static IEnumerator RequestAssetBundle(string assetBundleName, AsyncResource async, Action<string, float> onProgress)
    {
        var url = XPath.GetAbPath(assetBundleName);

        //webgl要用request加载
        if (XPlatform.Platform is XPlatform.EPlatform.WebGL or XPlatform.EPlatform.UnityEditor_AB)
        {
            var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerAssetBundle(url, 0);
            
            
            //Editor AB模式下做个假的加载过程，方便测试
            request.SendWebRequest();
            while (!request.isDone)
            {
                var progress = request.downloadProgress;
                onProgress?.Invoke(assetBundleName, progress);
                
                var eventData = new object[2];
                eventData[0] = assetBundleName;
                eventData[1] = progress;
                GameEventCenter.Send(GameEvent.OnAssetbundleDownload, eventData);
                
                yield return 0;
            }
                
            //Editor AB模式下做个假的加载过程，方便测试
            if (XPlatform.Platform == XPlatform.EPlatform.UnityEditor_AB)
            {
                yield return XResource.SimulateDownload(request, progress=>
                {
                    var eventData = new object[2];
                    eventData[0] = assetBundleName;
                    eventData[1] = progress;
                    GameEventCenter.Send(GameEvent.OnAssetbundleDownload, eventData);
                    
                    onProgress?.Invoke(assetBundleName, progress);
                });
            }
            
            if (!string.IsNullOrEmpty(request.error))
            {
                Debug.LogError("LoadAsset Error:" + request.error + " url:" + url);
                yield break;
            }
            async.AssetBundle = DownloadHandlerAssetBundle.GetContent(request);
        }
        //从本地加载
        else
        {
            var request = AssetBundle.LoadFromFileAsync(url);
            yield return request;
            async.AssetBundle = request.assetBundle;
        }
        
    }

    //Where we get all the dependencies and load them all.
    private static IEnumerator LoadDependencies(string assetBundleName, Action<string, float> onProgress)
    {
        if (_manifest == null)
        {
            Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
            yield break;
        }

        // Get dependecies from the AssetBundleManifest object..
        var dependencies = _manifest.GetAllDependencies(assetBundleName);
        if (dependencies.Length == 0)
        {
            yield break;
        }

        // Record and load all dependencies.
        Dependencies.Add(assetBundleName, dependencies);
        for (var i = 0; i < dependencies.Length; i++)
        {
            //如果依赖之前已经在加载中了，则不再加载，防止循环引用
            if (LoadingAssetBundles.TryGetValue(dependencies[i], out var inLoading) && inLoading)
            {
                continue;
            }
            //Debug.Log("["+assetBundleName+"]  load dependencies:"+dependencies[i]);
            yield return LoadAssetBundleInternal(dependencies[i], onProgress);
        }
    }

    // Load asset from the given assetBundle.
    private static IEnumerator LoadAsset(string assetBundleName, string assetName, AsyncResource async, Action<string, float> onProgress = null)
    {
        //Debug.Log("Loading " + assetName + " from " + assetBundleName + " bundle");
        yield return LoadAssetBundle(assetBundleName, onProgress);

        var bundle = GetLoadedAssetBundle(assetBundleName);
        if (bundle == null)
        {
            Debug.LogError("LoadAssetBundle Error bundle = null:" + assetBundleName);
            yield break;
        }

        var fullAssetPath = XPath.GetAssetNamePath(assetName);
        var obj = bundle.AssetBundle.LoadAsset(fullAssetPath);
        if (obj == null)
        {
            Debug.LogError("LoadAssetBundle Error async.Object = null:" + assetBundleName + "  " + assetName + "  " + fullAssetPath);
            yield break;
        }

        async.Object = obj;
    }


    public static IEnumerator LoadAssetBundleObject(string resourcePath, AsyncResource async)
    {
        //Editor下直接去读AssetDatabase
        if (XPlatform.Platform == XPlatform.EPlatform.UnityEditor)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(resourcePath))
            {
                var fileName = XPath.CONTENT_URL + resourcePath;
                var assetObject = UnityEditor.AssetDatabase.LoadAssetAtPath(fileName, typeof(Object));
                if (!assetObject)
                {
                    Debug.LogError($"load : {resourcePath} failed");
                }
                async.Object = assetObject;
            }
#endif
        }
        
        //LoadAssetBundle
        else
        {
            //shader特殊处理
            var assetBundleName = XPath.GetAbName(resourcePath);
            yield return LoadAsset(assetBundleName, resourcePath, async);
            //UnloadAssetBundle(assetBundleName);
        }
    }

    #region UNLOAD

    // Unload assetbundle and its dependencies.
    public static void UnloadAssetBundle(string assetBundleName)
    {
        UnloadDependencies(assetBundleName);
        UnloadAssetBundleInternal(assetBundleName);
    }

    private static void UnloadDependencies(string assetBundleName)
    {
        if (!Dependencies.TryGetValue(assetBundleName, out var dependencies))
        {
            return;
        }
        
        // Loop dependencies.
        foreach (var dependency in dependencies)
        {
            UnloadAssetBundleInternal(dependency);
        }
        Dependencies.Remove(assetBundleName);
    }

    private static void UnloadAssetBundleInternal(string assetBundleName)
    {
        var bundle = GetLoadedAssetBundle(assetBundleName);
        if (bundle == null)
        {
            return;
        }
        
        if (--bundle.ReferencedCount == 0)
        {
            bundle.AssetBundle.Unload(false);
            LoadedAssetBundles.Remove(assetBundleName);
        }
    }

    #endregion
}
