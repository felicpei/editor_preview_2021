
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class InversionDependenciesGraph : EditorWindow
{
	private static InversionDependenciesGraph _window;

	[MenuItem("Assets/Inversion Dependencies Graph", false, 39)]
	public static void Open()
	{
		if (_window == null)
		{
			_window = GetWindow<InversionDependenciesGraph>(true, "Inversion Dependencies Graph", true);
			if (!EditorPrefs.GetBool("firstOpenInversionDependenciesGraph"))
			{
				const int initWidth = 300;
				const int initHeight = 600;
				var x = (Screen.currentResolution.width - initWidth) / 2;
				var y = (Screen.currentResolution.height - initHeight) / 2;
				_window.position = new Rect(x, y, initWidth, initHeight);
				EditorPrefs.SetBool("firstOpenInversionDependenciesGraph", true);
			}
			_window.Analyze();
		}
		else
		{
			_window.Analyze();
		}
	}


	[MenuItem("Assets/Inversion Dependencies Graph", true)]
	private static bool OpenValidate()
	{
		var _object = Selection.activeObject;
		if (_object == null)
		{
			return false;
		}

		if (AssetDatabase.IsSubAsset(_object))
		{
			return false;
		}
		
		var path = AssetDatabase.GetAssetPath(_object);
		if (Directory.Exists(path))
		{
			return false;
		}
		
		return true;
	}

	private readonly List<Object> _dependerList = new();
	private Vector2 _scrollpos;

	private void Analyze()
	{
        var depend = AssetDatabase.GetAssetPath(Selection.activeObject);
        var prefabs = AssetDatabase.FindAssets("t:prefab", new[] { "Assets" });
        var scenes = AssetDatabase.FindAssets(" t:scene", new[] { "Assets" });

        var files = new List<string>();
        files.AddRange(prefabs);
        files.AddRange(scenes);
       
		_dependerList.Clear();
		for (var i = 0; i < files.Count; ++i)
		{
            var path = AssetDatabase.GUIDToAssetPath(files[i]);
            EditorUtility.DisplayCancelableProgressBar("analyze dependencies", path, (float)i / prefabs.Length);

            var list = AssetDatabase.GetDependencies(path, true);
            var index = System.Array.IndexOf(list, depend);
            if (index != -1)
            {
	            _dependerList.Add(AssetDatabase.LoadMainAssetAtPath(path));
            }
		}

		EditorUtility.ClearProgressBar();
		
		_scrollpos = Vector2.zero;
		titleContent.text = "Dependencies Of " + depend;
		Repaint();
	}

	private void OnGUI()
	{
		_scrollpos = EditorGUILayout.BeginScrollView(_scrollpos);
		
		foreach (var t in _dependerList)
		{
			var path = AssetDatabase.GetAssetPath(t);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUIUtility.SetIconSize(new Vector2(16, 16));
			GUILayout.Label(AssetDatabase.GetCachedIcon(path));
			
			if (GUILayout.Button(Path.GetFileNameWithoutExtension(path), EditorStyles.label))
			{
				Selection.activeObject = t;
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();
	}
	
	private void OnDestroy()
	{
		_window = null;
	}
}