

using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

public static class ResourceBuildTool
{
    private static readonly ResourceType[] BuildFilter;
    private static readonly ResourceType[] SceneFilterRemoves;
    private static readonly ResourceType[] DirectoryFilterRemoves;

    static ResourceBuildTool()
    {
        BuildFilter = new[]
        {
            ResourceType.text,
            ResourceType.scene,
            ResourceType.audio,
            //ResourceType.video,
            ResourceType.prefab,
            ResourceType.model,
            ResourceType.material,
            ResourceType.shader,
            ResourceType.font,
            ResourceType.texture,
            ResourceType.asset,
            ResourceType.bytes,
            ResourceType.spriteatlas
        };

        SceneFilterRemoves = new[]
        {
            ResourceType.audio,
            ResourceType.video,
            ResourceType.prefab,
            ResourceType.model,
            ResourceType.material,
            ResourceType.shader,
            ResourceType.spriteatlas,
            ResourceType.font,
            ResourceType.texture,
            ResourceType.bytes,
            ResourceType.asset,
        };

        DirectoryFilterRemoves = new[]
        {
            ResourceType.model,
            ResourceType.material,
            ResourceType.shader,
            ResourceType.font,
            ResourceType.texture,
            ResourceType.spriteatlas,
            ResourceType.video,
        };
    }

    private static readonly List<string> IgnorePaths = new();
    private static readonly List<string> RetainPaths = new();
    private static readonly List<string> ScenePaths = new();
    
    public static List<string> GetBuildResources(string pathname)
    {
        var resourceList = new List<string>();
        
        //强制忽略的文件夹
        IgnorePaths.Clear();
        
        //强制打包的文件夹
        RetainPaths.Clear();
        RetainPaths.Add("Entities");
        
        //场景文件夹
        ScenePaths.Clear();

        GetBuildResources(resourceList, pathname, BuildFilter);
        return resourceList;
    }

    //pathname => Asset/xxxx相对路径
    private static void GetBuildResources(List<string> resourceList, string pathname, params ResourceType[] filter)
    {
        if (!Directory.Exists(XPath.ProjectPathToFullPath(pathname)))
        {
            if (CollectionUtility.Contains(filter, XResourcesUtility.GetResourceTypeByPath(pathname)))
            {
                resourceList.Add(pathname);
            }
        }
        else
        {
            //忽略强制不打包的
            for (var i = 0; i < IgnorePaths.Count; i++)
            {
                if (pathname.Contains(IgnorePaths[i]))
                {
                    return;
                }
            }
            
            //必打包的文件夹
            var bRetain = false;
            for (var i = 0; i < RetainPaths.Count; i++)
            {
                if(pathname.Contains(RetainPaths[i]))
                {
                    bRetain = true;
                    break;
                }
            }

            if (bRetain)
            {
                filter = BuildFilter;
            }
            else
            {
                var bScene = false;
                for (var i = 0; i < ScenePaths.Count; i++)
                {
                    if(pathname.Contains(ScenePaths[i]))
                    {
                        bScene = true;
                        break;
                    }
                }

                //是否是scene
                if (bScene)
                {
                    filter = CollectionUtility.Remove(filter, SceneFilterRemoves);
                }
                else
                {
                    filter = CollectionUtility.Remove(filter, DirectoryFilterRemoves);
                }
            }

            var fileList = Directory.GetFiles(pathname, "*");
            foreach (var resource in fileList)
            {
                var path = XResourcesUtility.GetResourceTypeByPath(resource);
                if (CollectionUtility.Contains(filter, path))
                {
                    resourceList.Add(XPath.FullPathToProjectPath(resource));
                }    
            }

            var dictList = Directory.GetDirectories(pathname);
            foreach (var dictname in dictList)
            {
                GetBuildResources(resourceList, dictname, filter);
            }
        }
    }
    public static string DoShellCmd(string cmd,string arg)
    {
        try
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName = cmd,
                    Arguments = arg,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            p.Start();
            p.WaitForExit();
            var strResult = p.StandardOutput.ReadToEnd();
            p.Close();
            return strResult;
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.Log(ex);
        }

        return "";
    }
}
