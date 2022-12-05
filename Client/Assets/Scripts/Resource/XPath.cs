
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class XPath
{
    public const string FULL_CONTENT_URL = "Client/Assets/Content/";
    public const string CONTENT_URL = "Assets/Content/";
    public const string CONTENT_NAME = "Content";
    
    public const string TextZipName = "textzip";
    public const string TablesPath = "Tables/";
    public const string TableExtension=  ".tab";
    
    public static readonly string AssetsPath;
    public static readonly string ContentPath;
    public static readonly string AssetBundlePath;
    public static readonly string RootPath;
    public static readonly string ProjectPath;
    public static readonly string ConfigPath;
    
    static XPath()
    {
        AssetsPath = FormatPath(Application.dataPath).FormatPath() + "/";
        AssetBundlePath = Application.streamingAssetsPath.FormatPath() + "/";
        ContentPath = AssetsPath + CONTENT_NAME + "/"; 
        ProjectPath = AssetsPath.GetParentPath().FormatPath();
       
#if UNITY_WEBGL && !UNITY_EDITOR
        ProjectPath = AssetsPath;
        RootPath = AssetsPath;
#else
        ProjectPath = AssetsPath.GetParentPath().FormatPath();
        RootPath = ProjectPath.GetParentPath().FormatPath();
#endif
        ConfigPath = RootPath + "config/";
        
        Debug.Log("------------- Init Path --------------");
        Debug.Log("AssetsPath:" + AssetsPath);
        Debug.Log("AssetBundlePath:" + AssetBundlePath);
        Debug.Log("ContentPath:" + ContentPath);
        Debug.Log("ProjectPath:" + ProjectPath);
        Debug.Log("RootPath:" + RootPath);
        Debug.Log("ConfigPath:" + ConfigPath);
        Debug.Log("------------- Init Path End --------------");

        InitAssetBundlePath();
    }

    public static string Combine(string path1, string path2)
    {
        return Path.Combine(path1, path2).FormatPath();
    }

    public static string FullPathToProjectPath(string fullPath)
    {
        var path= fullPath.FormatPath().Replace(ProjectPath, "");
        return path;
    }

    public static string FullPathToResourcePath(string fullPath)
    {
        var path= fullPath.FormatPath().Replace(ContentPath, "");
        return path;
    }
    
    public static string ProjectPathToFullPath(string path)
    {
        return Combine(ProjectPath, path);
    }

    private static readonly Dictionary<string, string> _assetbundlePath = new();
    private static readonly List<string> _singleFilePath = new();
    public static void InitAssetBundlePath()
    {
        _singleFilePath.Clear();
        _assetbundlePath.Clear();

        _assetbundlePath["3rds"] = "3rds";
        _assetbundlePath["CubeScene"] = "scenes_cube";
        _assetbundlePath["Common"] = "common";
        _assetbundlePath["Shaders"] = "shaders";
        _assetbundlePath["UI"] = "ui";
    }
    
    
    public static string GetAbName(string path)
    {
        var extension = ".ab";
        
        //path = resouercex 下的path，例如：Shaders/DefaultResourcesExtra/Prrticle Add.shader
        path = path.FormatPath();
        var type = XResourcesUtility.GetResourceTypeByPath(path);
        
        //检查是否是按文件夹自定义的assetbundleName
        if (type is ResourceType.scene or  ResourceType.video)
        {
            var strAbName = path.GetUniquePath();
            return strAbName.ToLowerInvariant() + extension;
        }
        
        //is single file
        for (var i = 0; i < _singleFilePath.Count; i++)
        {
            if (path.IndexOf(_singleFilePath[i], StringComparison.Ordinal) == 0)
            {
                var strAbName = path.GetUniquePath();
                return strAbName + extension;
            }
        }
        
        //看是否配置了路径
        foreach (var kv in _assetbundlePath)
        {
            if (path.IndexOf(kv.Key, StringComparison.Ordinal) == 0)
            {
                return kv.Value.ToLower() + extension;
            }
        }
        
        path = path.Replace(CONTENT_URL, "");
        var items = path.Split('/');

        //Content根目录下按文件打包，有子文件则按文件夹打包
        if (items.Length <= 2)
        {
            var dotIndex = path.LastIndexOf(".", StringComparison.Ordinal);
            if (dotIndex != -1)
            {
                path = path.Substring(0, dotIndex);
            }
        }
        else
        {
            //使用文件夾
            path = path.Replace(items[^1], "");
        }
        
        path = path.GetUniquePath();
        return path + extension;
    }
    
    public static string GetUniquePath(this string path)
    {
        var fileName = FormatPath(path).Replace("/", "_").Replace(".", "_").ToLower();
        return fileName;
    }

    //根据fullpath生成唯一的文件名
    public static string GetUniqueNameByFullpath(this string path)
    {
        //zip文件要去掉 game-xx/Assets/resourcex 否则不同的平台会有问题
        path = path.FormatPath().Replace(XPath.FULL_CONTENT_URL, "");
        return path.Replace(RootPath, "").GetUniquePath();
    }

    public static string GetAssetNamePath(string assetName)
    {
        var path = assetName;
        if (assetName != "AssetBundleManifest")
        {
            path = CONTENT_URL + assetName;
        }
        return path;
    }

    public static string GetAbPath(string resource)
    {
        return Combine(AssetBundlePath, resource);
    }
    
    public static string FormatPath(this string path)
    {
        return path.Replace('\\', '/');
    }

    public static string GetParentPath(this string path)
    {
        var lastIndex = path.LastIndexOf("/", StringComparison.Ordinal);
        if (lastIndex == -1)
        {
            Debug.LogError("GetParentPath Error, last '/' not found, path:" + path);
            return path;
        }
  
        //排除最后以后是/的情况
        if (lastIndex + 1 == path.Length)
        {
            return GetParentPath(path.Substring(0, lastIndex));
        }
        return path.Substring(0, lastIndex + 1);
    }
}

