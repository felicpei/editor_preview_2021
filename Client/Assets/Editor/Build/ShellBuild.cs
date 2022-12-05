using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;

public static class ShellBuild
{
	private static readonly string _buildPathIOS;
    private static readonly string _buildPathAndroid;
    private static readonly string _buildPathWindows;
    
    private static readonly string _buildOutWebGL;
    
    private static BuildOptions _buildOption;
    static ShellBuild()
    {
	    _buildPathIOS = XPath.RootPath + "BuildIOS/Ios";
		_buildPathAndroid = XPath.RootPath + "Build_Android/Android.apk";
        _buildPathWindows = XPath.RootPath + "Build_Windows/PreviewResource.exe";
        
        _buildOutWebGL =  XPath.RootPath + "nginx/html/";
        _buildOption = BuildOptions.None;
	}

    private static void XSwitchPlatform(BuildTargetGroup targetGroup, BuildTarget buildTarget)
    {
		if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
		{
			EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, buildTarget);
		}
	}

    private static void SetBuildSettings(bool bDebug = false)
	{
		_buildOption = BuildOptions.None;
		_buildOption |= BuildOptions.CompressWithLz4HC;
		if (bDebug)
		{
			_buildOption |= BuildOptions.Development;
			//_buildOption |= BuildOptions.ConnectWithProfiler;
#if !UNITY_WEBGL
			_buildOption |= BuildOptions.AllowDebugging;
#endif
		}
	}

    [MenuItem("XEngine/Build/BuildAsset", false, 100)]
    public static void BuildAsset()
    {
	    Build(_buildPathWindows, BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, true, false);
    }
  
    [MenuItem("XEngine/Build/Win_Debug", false, 1)]
    public static void Build_StandaloneDebug()
    {
        Build(_buildPathWindows, BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, true);
    }
    
    [MenuItem("XEngine/Build/Win_Release", false, 2)]
    public static void Build_StandaloneRelease()
    {
	    Build(_buildPathWindows, BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, false);
    }
   
    [MenuItem("XEngine/Build/Android_Debug", false, 3)]
    public static void Build_AndroidDebug()
    {
	    if (File.Exists(_buildPathAndroid))
	    { 
		    File.Delete(_buildPathAndroid);
	    }
	    Build(_buildPathAndroid, BuildTarget.Android, BuildTargetGroup.Android, true);
    }
  
    [MenuItem("XEngine/Build/Android_Release", false, 4)]
    public static void Build_AndroidRelease()
    {
	    if (File.Exists(_buildPathAndroid))
	    { 
		    File.Delete(_buildPathAndroid);
	    }
	    Build(_buildPathAndroid, BuildTarget.Android, BuildTargetGroup.Android, false);
    }
    
    [MenuItem("XEngine/Build/WebGL_Debug", false, 5)]
    public static void Build_WebGLDebug()
    {
	    DeleteDirectory(_buildOutWebGL);
	    Build(_buildOutWebGL, BuildTarget.WebGL, BuildTargetGroup.WebGL, true);
    }

    [MenuItem("XEngine/Build/WebGL_Release", false, 6)]
    public static void Build_WebGLRelease()
    {
	    DeleteDirectory(_buildOutWebGL);
	    Build(_buildOutWebGL, BuildTarget.WebGL, BuildTargetGroup.WebGL, false);
    }
    
    /*
    [MenuItem("XEngine/ShellBuild/IOS_Debug", false, 1)]
    public static void CommandLineBuildIOSDebug()
    {
	    Build(_buildPathIOS, BuildTarget.iOS, BuildTargetGroup.iOS, true);
    }
    
    [MenuItem("XEngine/ShellBuild/IOS_Release", false, 2)]
    public static void CommandLineBuildIOS()
    {
        Build(_buildPathIOS, BuildTarget.iOS, BuildTargetGroup.iOS, false);
    }


    [MenuItem("XEngine/ShellBuild/Android_Debug", false, 3)]
    public static void CommandLineBuildAndroidDebug()
    {
	    Build(_buildPathAndroid, BuildTarget.Android, BuildTargetGroup.Android, true);
    }
    

    [MenuItem("XEngine/ShellBuild/Android_Release", false, 4)]
    public static void CommandLineBuildAndroid()
    {
	    Build(_buildPathAndroid, BuildTarget.Android, BuildTargetGroup.Android, false);
    }
    */

    private static void Build(string buildPath, BuildTarget target, BuildTargetGroup targetGroup, bool bDebug, bool bBuildPlayer = true)
    {

	    //使用更小的IL2CPP压缩方式
	    //PlayerSettings.il2CppCodeGeneration = Il2CppCodeGeneration.OptimizeSize;
	    
	    SetBuildSettings(bDebug);
		XSwitchPlatform(targetGroup, target);
		
		//临时禁用除了gameStart的其他场景，减少体积
		//DisableScene();
		
		//refresh asset
		AssetDatabase.Refresh();

		//build assets
		ResourcesBuilder.BuildAsset(XPath.AssetBundlePath, target);

		/*//copy config
		if (target == BuildTarget.WebGL)
		{
			var webglConfigPath = _buildOutWebGL + "/config";
			DeleteDirectory(webglConfigPath);
			CopyDirectory(XPath.ConfigPath, webglConfigPath, true);
		}
		*/

		if (bBuildPlayer)
		{
			BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath, target, _buildOption);
			EditorUtility.ClearProgressBar();
		}
        
        //EnableScene();
    }
    
    
    /*private static readonly List<EditorBuildSettingsScene> _sceneAssets = new();
    public static void DisableScene()
    {
	    _sceneAssets.Clear();
	    for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
	    {
		    _sceneAssets.Add(EditorBuildSettings.scenes[i]);
	    }

	    var clearList = new List<EditorBuildSettingsScene>();
	    clearList.Add(EditorBuildSettings.scenes[0]);
	    EditorBuildSettings.scenes = clearList.ToArray();
	    
	    AssetDatabase.Refresh();
    }
    
    public static void EnableScene()
    {
	    EditorBuildSettings.scenes = _sceneAssets.ToArray();
	    AssetDatabase.Refresh();
    }*/

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
	    // Get information about the source directory
	    var dir = new DirectoryInfo(sourceDir);

	    // Check if the source directory exists
	    if (!dir.Exists)
	    {
		    throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
	    }

	    // Cache directories before we start copying
	    var dirs = dir.GetDirectories();

	    // Create the destination directory
	    Directory.CreateDirectory(destinationDir);

	    // Get the files in the source directory and copy to the destination directory
	    foreach (var file in dir.GetFiles())
	    {
		    var targetFilePath = Path.Combine(destinationDir, file.Name);
		    file.CopyTo(targetFilePath);
	    }

	    // If recursive and copying subdirectories, recursively call this method
	    if (recursive)
	    {
		    foreach (var subDir in dirs)
		    {
			    var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
			    CopyDirectory(subDir.FullName, newDestinationDir, true);
		    }
	    }
    }

    private static void DeleteDirectory(string path)
    {
	    if (Directory.Exists(path))
	    {
		    Directory.Delete(path, true);
	    }
    }
}
