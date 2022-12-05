using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEditor;
using UnityEngine;

public static class ResourcesBuilder
{
    public static void BuildAsset(string assetPath, BuildTarget target)
    {
        if (!Directory.Exists(assetPath))
        {
            Directory.CreateDirectory(assetPath);
        }
 
        var entityPath = Path.Combine(assetPath, "EntityScenes");
        if (!Directory.Exists(entityPath))
        {
            Directory.CreateDirectory(entityPath);
        }
         
        //refresh assets
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        //先buildtext
        BuildText();
        
        //set assetbundleNames
        if (!AssetBundleNameTools.RefreshAssetBundleName())
        {
            return;
        }
       
        //build
        try
        {
            //lz4 格式 压缩
            const BuildAssetBundleOptions buildAssetOptions = BuildAssetBundleOptions.ChunkBasedCompression;
            var assetManifest = BuildPipeline.BuildAssetBundles(assetPath, buildAssetOptions, target);
            if (assetManifest == null)
            { 
                Debug.LogError("BuildPipeline.BuildAssetBundles Error, assetManifest is null");
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            EditorUtility.ClearProgressBar();
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("XEngine/Build/BuildText", false, 100)]
    public static void BuildText()
    {
        var list = new List<string>();
        
        //所有的lua和配置打包
        //list.AddRange(Directory.GetFiles(XPath.RootPath + "lua", "*.*", SearchOption.AllDirectories));
        list.AddRange(Directory.GetFiles(XPath.ProjectPath + "Assets/", "*.tab", SearchOption.AllDirectories));
        list.AddRange(Directory.GetFiles(XPath.ProjectPath + "Assets/", "*.cfg", SearchOption.AllDirectories));

        //全部配置表打包进zip
        var zipFilePath = XPath.Combine(XPath.AssetBundlePath, XPath.TextZipName);
        FilesToZip(list, zipFilePath);

        Debug.Log("build text ok. " + list.Count + " items.");
    }


    private static void FilesToZip(List<string> list, string zipFilePath)
    {
        if (File.Exists(zipFilePath))
        {
            File.Delete(zipFilePath);
        }

        using var zipFile = new FileStream(zipFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        
        using (var zipStream = new ZipOutputStream(zipFile))
        {
            foreach (var p in list)
            {
                //path需要只保留 Assets/resourcex/后面的， 否则webgl拿不到目录
                var path  = p.FormatPath();
                    
                //读取配置表内容并加密
                string text = FileHelper.GetTableFromFile(path);
                if (path.ToLower().EndsWith(".tab"))
                {
                    text = TableDatabase.RemoveComment(text, true);
                }

                var ms = new MemoryStream();
                using (var writer = new StreamWriter(ms, new System.Text.UTF8Encoding(true)))
                {
                    writer.Write(text);
                    writer.Close();
                }
                var buff = ms.ToArray();
                if (path.ToLower().EndsWith(".tab"))
                {
                    buff = FileHelper.CopyFrom(buff, 3);                //保存资源加密
                }
                    
                //去掉工程路径, 再转为唯一文件名
                var fileName = path.GetUniqueNameByFullpath();
                var zipEntry = new ZipEntry(fileName);
                zipStream.PutNextEntry(zipEntry);
                zipStream.SetLevel(6);  //1-9
                zipStream.Write(buff, 0, buff.Length);
                zipStream.Flush();
            }
            zipStream.Flush();
            zipStream.Close();
        }

        zipFile.Close();
    }
}

