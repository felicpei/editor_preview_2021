
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundleNameTools
{
    private static readonly HashSet<string> ResourceSet = new();
    private static readonly List<string> AllowShadersList = new();

    static AssetBundleNameTools()
    {
        
    }
    
    
    [MenuItem("XEngine/Reset All AssetBundleName")]
    public static void ResetAllAssetBundleName()
    {
        XPath.InitAssetBundlePath();
        ClearAssetbundleName();
        AssetDatabase.Refresh();
        RefreshAssetBundleName();
        AssetDatabase.Refresh();
    }

    public static void ClearAssetbundleName()
    {
        var abNameArr = AssetDatabase.GetAllAssetBundleNames();
        for (var i = 0; i < abNameArr.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Remove All AssetBundleName", abNameArr[i], (float)i / abNameArr.Length);
            AssetDatabase.RemoveAssetBundleName(abNameArr[i], true);
        }
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
    }
    
    public static bool RefreshAssetBundleName()
    {
        //set abname
        var resourceList = ResourceBuildTool.GetBuildResources(XPath.FullPathToProjectPath(XPath.ContentPath));
        CollectionUtility.Insert(ResourceSet, resourceList);

        if (!SetAssetsBundleName(ResourceSet))
        {
            Debug.LogError("Error SetAssetsBundleName失敗");
            return false;
        }
        EditorUtility.ClearProgressBar();
        return true;
    }

    private static bool SetAssetsBundleName(HashSet<string> assetPaths)
    {
        var progress = 0;

        //step 1: 遍历所有依赖, 缓存资源被依赖次数
        var dependCount = new Dictionary<string, int>();
        var dependResourceList = new Dictionary<string, List<string>>();
        foreach (var resource in assetPaths)
        {
            var dependencies = AssetDatabase.GetDependencies(resource, true);
            foreach (var dependName in dependencies)
            {
                if (!dependCount.ContainsKey(dependName))
                {
                    dependCount[dependName] = 1;
                    dependResourceList[dependName] = new List<string>();
                }
                else
                {
                    dependCount[dependName]++;
                }
                dependResourceList[dependName].Add(resource);
            }
            ++progress;
            ShowProgressBar("Get DependFileList", resource, progress / (float)assetPaths.Count);
        }

        
        progress = 0;
        var enumerator = dependCount.GetEnumerator();
        using (enumerator)
        {
            while (enumerator.MoveNext())
            {
                //Debug.Log("resource:" + e.Current.Key + "  count:" + e.Current.Value);
                var resource = enumerator.Current.Key.FormatPath();
                var type = XResourcesUtility.GetResourceTypeByPath(enumerator.Current.Key);
                var importer = AssetImporter.GetAtPath(resource);
                if (importer == null)
                {
                    Debug.LogError("Error Resource importer is Null. --> " + resource);
                    return false;
                }
                switch (type)
                {
                    case ResourceType.texture:
                        var textureImporter = importer as TextureImporter;
                        if (textureImporter != null && textureImporter.textureType == TextureImporterType.Sprite)
                        {
                            importer.assetBundleName = null;
                        }
                        else
                        {
                            importer.assetBundleName = XPath.GetAbName(resource.Replace(XPath.CONTENT_URL,""));
                        }
                        break;
                    case ResourceType.scene:
                    case ResourceType.model:
                    case ResourceType.material:
                    case ResourceType.audio:
                    case ResourceType.font:
                    case ResourceType.video:
                    case ResourceType.asset: 
                    case ResourceType.bytes:
                    case ResourceType.spriteatlas:
                    case ResourceType.prefab:
                        importer.assetBundleName =  XPath.GetAbName(resource.Replace(XPath.CONTENT_URL,""));
                        break;
                    case ResourceType.shader:
                   
                        if (!CheckShader(resource))
                        {
                            if (dependResourceList.ContainsKey(resource))
                            {
                                for (int i = 0; i < dependResourceList[resource].Count; i++)
                                {
                                    Debug.LogWarning("Error, 规定外的shader包含的资源:" + dependResourceList[resource][i] + " ShaderName:" + resource);
                                }
                            }
                        }
                        
                        importer.assetBundleName =  XPath.GetAbName(resource.Replace(XPath.CONTENT_URL,""));
                        break;

                    case ResourceType.text:
                    case ResourceType.script:
                    case ResourceType.folder:
                    {
                        break;
                    }

                    default:
                    {
                        importer.assetBundleName =  XPath.GetAbName(resource.Replace(XPath.CONTENT_URL,""));
                        Debug.LogError("unknown depend resource : " + resource + " type:" + type);
                        break;
                    }
                }
                ++progress;
                ShowProgressBar("Set Resource AssetBundleName", resource, progress / (float)dependCount.Count);
            }
        }

        return true;
    }
    
    
    private static void ShowProgressBar(string str1, string strInfo, float progress)
    {
        var bCancel = EditorUtility.DisplayCancelableProgressBar(str1, strInfo, progress);
        if (bCancel)
        {
            EditorUtility.ClearProgressBar();
            throw new Exception("User break!");
        }
    }

    private static bool CheckShader(string resource)
    {
        /*
        for (int i = 0; i < AllowShadersList.Count; i++)
        {
            if (resource.Replace(XPath.CONTENT_URL, "") == AllowShadersList[i])
            {
                return true;
            }
        }

        return false;
        */
        return true;
    }
}
