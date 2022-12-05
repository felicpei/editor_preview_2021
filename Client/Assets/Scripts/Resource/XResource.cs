using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AsyncResource
{
    public Object Object;
    public AssetBundle AssetBundle;
    public string Text;
    public byte[] bytes;
    public Texture2D Texture;
}

public class XResource : MonoBehaviour
{
    public const int EDITOR_DOWNLOAD_SPEED = 10000 * 1000; //10k/fps

    private static XResource _inst;
    public static void Init()
    {
        if (_inst != null)
        {
            return;
        }

        var gameObj = new GameObject("XResource");
        DontDestroyOnLoad(gameObj);
        _inst = gameObj.AddComponent<XResource>();
    }
    
    public static void Load(string resource, Action<Object> notify)
    {
        if (string.IsNullOrEmpty(resource))
        {
            Debug.LogError("resource path is null");
            return;
        }

        LoadObject(resource, obj => { notify?.Invoke(obj); });
    }

    public static void LoadGameObject(string resource, Action<GameObject> callback)
    {
        Load(resource, obj =>
        {
            var gameObj = obj.InstantiateGo();
            callback.Invoke(gameObj);
        });
    }

    private static void LoadObject(string resource, Action<Object> callBack)
    {
        _inst.StartCoroutine(LoadObjectAsync(resource, callBack));
    }

    public static IEnumerator LoadObjectAsync(string resource, Action<Object> callBack)
    {
        var async = new AsyncResource();
        yield return XAssetBundle.LoadAssetBundleObject(resource, async);
        callBack(async.Object);
    }

    public static IEnumerator LoadScene(string levelName, LoadSceneMode mode, Action<string> onFinished = null, Action<string, float> onProgress = null)
    {
        Debug.Log("LoadScene:" + levelName);

        var assetBundleName = XPath.GetAbName(levelName);

        //Load AssetBundle
        if (XPlatform.Platform != XPlatform.EPlatform.UnityEditor)
        {
            yield return XAssetBundle.LoadAssetBundle(assetBundleName, onProgress);
        }

        var sceneName = XPath.GetAssetNamePath(levelName);
        //LoadScene
        var request = SceneManager.LoadSceneAsync(sceneName, mode);
        while (!request.isDone)
        {
            yield return 0;
        }
        
        onFinished?.Invoke(sceneName);
        Debug.Log("LoadScene Finished:" + levelName);
    }

    private static readonly Dictionary<string, byte[]> _zipTextCache = new();

    public static IEnumerator CacheTableZip(Action<string, float> onProgress)
    {
        if (XPlatform.Platform == XPlatform.EPlatform.UnityEditor)
        {
            yield break;
        }

        //Dbg.Log("Cache Text Start:" + Time.time);

        var bFinished = false;
        yield return RequestZipFile(XPath.TextZipName, zip =>
        {
            //一次性解压并把所有内容放在内存
            for (var i = 0; i < zip.Count; i++)
            {
                var name = zip[i].Name;
                var zipEntry = zip[i];
                _zipTextCache[name] = ZipEntityToMemoryStream(zip, zipEntry);
                //Dbg.Log("Cache Text:" + name + " size:" + _zipTextCache[name].Length);
            }

            Debug.Log("Cache Text  Finished zipCount:" + zip.Count);
            bFinished = true;
        }, progress => { onProgress?.Invoke("配置文件", progress); });

        yield return new WaitUntil(() => bFinished);

        //Dbg.Log("Cache Text  Finished:" + Time.time);
    }

    //读取配置表
    public static string LoadTableText(string path)
    {
        //editor模式下, 直接用file读
        if (XPlatform.Platform == XPlatform.EPlatform.UnityEditor)
        {
            var fullPath = XPath.ContentPath + path;
            return FileHelper.GetTableFromFile(fullPath);
        }

        var fileName = path.GetUniqueNameByFullpath();
        if (_zipTextCache.TryGetValue(fileName, out var bytes))
        {
            return FileHelper.DecodeTable(bytes);
        }

        Debug.LogError("读取配置表缓存失败:" + path + " filename:" + fileName);
        return "";
    }

    public static Object LoadEditorAsset(string resourcePath)
    {
        //Editor下直接去读AssetDatabase
        if (XPlatform.InEditor)
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

                return assetObject;
            }
#endif
        }

        Debug.LogError("LoadEditorAsset Error, 当前不是Editor:" + XPlatform.Platform);
        return null;
    }

    //-------------------- GC START --------------------//
    public static IEnumerator DoGc(float sec = 0)
    {
#if !UNITY_WEBGL
        yield return new WaitForSeconds(sec);
        yield return Resources.UnloadUnusedAssets();
        GC.Collect();
#else
        yield break;
#endif
    }

    //-------------------- GC END --------------------//


    //-------------------- 外部资源加载Start --------------------//

    //加载二进制文件
    public static void LoadBytes(string fullPath, Action<byte[]> callBack, Action onError)
    {
        _inst.StartCoroutine(LoadBytesAsync(fullPath, callBack, onError));
    }

    private static IEnumerator LoadBytesAsync(string fullPath, Action<byte[]> callBack, Action onError)
    {
        var async = new AsyncResource();
        yield return RequestBytes(fullPath, async, onError, _ => { });
        callBack(async.bytes);
    }

    public static IEnumerator RequestBytes(string fullPath, AsyncResource async, Action onError, Action<float> onProgress)
    {
        var request = UnityWebRequest.Get(fullPath);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SendWebRequest();

        while (!request.isDone)
        {
            onProgress?.Invoke(request.downloadProgress);
            yield return 0;
        }

        //Editor AB模式下做个假的加载过程，方便测试
        if (XPlatform.Platform == XPlatform.EPlatform.UnityEditor_AB)
        {
            yield return SimulateDownload(request, onProgress);
        }

        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError("RequestFile:" + request.error + " path:" + fullPath);
            onError.Invoke();
        }
        else
        {
            var data = request.downloadHandler.data;
            async.bytes = data;
        }
        request.Dispose();
    }

    //一次性加载多个文本文件，并按顺序返回
    public static void LoadTextList(List<string> fullPaths, Action<List<string>> callBack)
    {
        _inst.StartCoroutine(LoadTextListAsync(fullPaths, callBack));
    }

    public static IEnumerator LoadTextListAsync(List<string> fullPaths, Action<List<string>> callBack)
    {
        var result = new List<string>();
        for (var i = 0; i < fullPaths.Count; i++)
        {
            var async = new AsyncResource();
            yield return RequestText(fullPaths[i], async);
            result.Add(async.Text);
        }

        callBack(result);
    }

    //加载文本文件
    public static void LoadText(string fullPath, Action<string> callBack)
    {
        _inst.StartCoroutine(LoadTextAsync(fullPath, callBack));
    }


    private static IEnumerator LoadTextAsync(string fullPath, Action<string> callBack)
    {
        var async = new AsyncResource();
        yield return RequestText(fullPath, async);
        callBack(async.Text);
    }

    public static IEnumerator RequestText(string fullPath, AsyncResource async)
    {
        var request = UnityWebRequest.Get(fullPath);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError("Request Text Error:" + request.error + " url:" + fullPath);
        }
        else
        {
            var buffer = request.downloadHandler.data;
            if (buffer == null)
            {
                Debug.LogError("GetResourceText Error, text = null" + fullPath);
                async.Text = "";
            }
            else
            {
                var text = FileHelper.DecodeUTF8(buffer);
                async.Text = text;
            }
        }
    }


    private static Stream _zipStream;

    //下载zip文件
    public static IEnumerator RequestZipFile(string zipName, Action<ZipFile> callback, Action<float> onProgress)
    {
        var path = XPath.Combine(XPath.AssetBundlePath, zipName);
        var async = new AsyncResource();
        yield return RequestBytes(path, async, () => { callback.Invoke(null); }, onProgress);

        var bytes = async.bytes;
        using (var ms = new MemoryStream(bytes))
        {
            using (var reader = new BinaryReader(ms))
            {
                reader.ReadBytes(bytes.Length);
                callback(new ZipFile(ms));
            }
        }
    }

    //ZIP中读取文本
    public static string GetTextFromZip(ZipFile zip, string resourcePath)
    {
        var buffer = GetFileInZip(zip, resourcePath);
        if (buffer != null)
        {
            return FileHelper.DecodeTable(buffer);
        }

        Debug.LogError("Get Text From Zip Error, path:" + resourcePath);
        return "";
    }

    //从ZIP中读取二进制
    private static byte[] GetFileInZip(ZipFile zip, string resourcePath)
    {
        using (zip)
        {
            if (zip == null)
            {
                Debug.LogError("GetFileInZip error zip is null");
                return null;
            }

            Debug.Log("get file in zip:" + resourcePath);
            var fileName = resourcePath.GetUniqueNameByFullpath();
            var zipEntity = zip.GetEntry(fileName);
            if (zipEntity == null)
            {
                Debug.LogError("GetFileInZip Error zipEntity is null, fileName:" + fileName);
                return null;
            }

            return ZipEntityToMemoryStream(zip, zipEntity);
        }
    }

    //把ZipEntity转为二进制
    private static byte[] ZipEntityToMemoryStream(ZipFile zip, ZipEntry zipEntry)
    {
        var stm = zip.GetInputStream(zipEntry);
        using (stm)
        {
            var mem = new MemoryStream();
            const int buffSize = 4096;
            var buf = new byte[buffSize];

            while (true)
            {
                var reads = stm.Read(buf, 0, buffSize);
                if (reads > 0)
                {
                    mem.Write(buf, 0, reads);
                }

                if (reads < buffSize)
                {
                    break;
                }
            }

            return mem.ToArray();
        }
    }
    //-------------------- 外部资源加载End --------------------//

    //模拟下载过程
    public static IEnumerator SimulateDownload(UnityWebRequest request, Action<float> onProgress)
    {
        ulong curBytes = 0;
        while (curBytes < request.downloadedBytes)
        {
            curBytes += EDITOR_DOWNLOAD_SPEED; //模拟网速
            onProgress?.Invoke(curBytes / (float)request.downloadedBytes);
            yield return new WaitForFixedUpdate();
        }
    }
}