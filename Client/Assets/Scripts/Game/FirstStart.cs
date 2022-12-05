using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FirstStart : MonoBehaviour
{
	private static FirstStart Inst;
	public InputField Input;
	public GameObject SpriteAni;
	public GameObject PrefabContent;
	
	
	private void Awake()
	{
		Inited = false;
		Inst = this;
		
		Input.onSubmit.AddListener(OnSubmitInput);
	}

	private void OnSubmitInput(string text)
	{
		ShowPreview(text);
	}

	private void Start()
	{
		StartCoroutine(Init());
	}


	public static bool Inited;
	public static IEnumerator Init(bool bFromLauncher = false)
	{
		if (Inited)
		{
			yield break;
		}

		Inited = true;
		
		yield return XPlatform.Init(false);
		
		//初始化AB
		yield return XAssetBundle.Initialize(null);
		
		XResource.Init();
		//AtlasLoader.Init();
		
		//加载配置表
		//yield return XResource.CacheTableZip(SetLoadingProgress);
	}

	public static void ShowPreview(string url)
	{
		//去掉前半截：D:\Works\editor_preview\Client\Assets\Content\
		//保留后半截：CartoonFX\CFX Prefabs\Explosions\CFX_Enemy_Explosion (Colored).prefab
		Clear();

		var urlPath = url.FormatPath();

		try
		{
			var extension = Path.GetExtension(url);
			switch (extension)
			{
				//prefab
				case ".prefab":
				{
					XResource.LoadGameObject(urlPath, gameObj =>
					{
						gameObj.transform.SetParent(Inst.PrefabContent.transform, false);
					
						//如果是粒子，则改为循环，方便预览
						var particles = gameObj.GetComponentsInChildren<ParticleSystem>();
						foreach (var ps in particles)
						{
							var main = ps.main;
							main.loop= true;
						}
					});
					break;	
				}
				
				//材质球
				case ".mat":
					XResource.Load(urlPath, obj =>
					{
						var mat = obj as Material;
						if (mat != null && mat.shader.name.Contains("FlipBook"))
						{
							var renderer = Inst.SpriteAni.GetComponent<MeshRenderer>();
							renderer.sharedMaterial = mat;
							Inst.SpriteAni.SetActiveSafe(true);
							return;
						}
						throw new Exception("所选的材质球不是序列帧动画类型，无法预览");
					});
					break;
				default:
					throw new Exception("不支持的文件类型:" + extension);
			}
		}
		catch (Exception e)
		{
			//Dbg.LogError(e);
		}
	}
	
	private static void Clear()
	{
		Inst.SpriteAni.SetActiveSafe(false);
		Inst.PrefabContent.DestroyAllChild();
	}

	private void OnDestroy()
	{
		Inst = null;
	}
}