using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.SceneManagement;

public class EditorTools
{
    [MenuItem("Assets/Copy Full Path ", false, 15)]
    public static void CopyPath()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        EditorGUIUtility.systemCopyBuffer = path;
    }

    [MenuItem("Assets/Copy Resource Path", false, 16)]
    public static void CopyResourcePath()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        var contentPath = XPath.CONTENT_URL;
        if (path.StartsWith(contentPath))
        {
            path = path.Remove(0, contentPath.Length);
        }

        EditorGUIUtility.systemCopyBuffer = path;
    }

    [MenuItem("Assets/Copy All Resource Path", false, 17)]
    public static void CopyAllResourcePath()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        var fullpath = XPath.ProjectPathToFullPath(path);
        if (Directory.Exists(fullpath))
        {
            var paths = Directory.GetFiles(fullpath, "*", SearchOption.TopDirectoryOnly);
            var allpath = string.Empty;
            for (var i = 0; i < paths.Length; ++i)
            {
                if (Path.GetExtension(paths[i]) == ".meta")
                {
                    continue;
                }

                allpath += XPath.CONTENT_URL + paths[i];
                if (i < paths.Length - 1)
                {
                    allpath += "\r\n";
                }
            }

            EditorGUIUtility.systemCopyBuffer = allpath;
        }
    }
}