using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ESceneId
{
    MainCity = 0,
    Test = 1,
}

public class SceneLoader
{
    public static SceneBase ActiveScene { get; protected set; }

    public static Transform Root
    {
        get
        {
            if (ActiveScene == null || ActiveScene.gameObject == null)
            {
                return null;
            }
            return ActiveScene.transform;
        }
    }

    private static Type GetSceneType(SceneDeploy sceneDeploy)
    {
        Type typeScene = null;
        
        var sceneClass = sceneDeploy.sceneClass;
        if (!string.IsNullOrEmpty(sceneClass))
        {
            typeScene = Type.GetType(sceneClass);
            if (typeScene == null)
            {
                Dbg.LogError(string.Format("scene class : {0} not exist", sceneClass));
            }
        }

        if (typeScene == null)
        {
            typeScene = typeof(SceneBase);
        }

        return typeScene;
    }

    public static void LoadMainCity(Action onFinish)
    {
        Load((int)ESceneId.MainCity, _ =>
        {
            onFinish.Invoke();
        });
    }
    
    public static void Load(int sceneId, Action<SceneBase> finishAction = null)
    {
        //Debug.Log("start load scene");
        var sceneDeploy = TableMgr.GetDeploy<SceneDeploy>(sceneId);
        if (sceneDeploy == null)
        {
            finishAction?.Invoke(null);
            Dbg.LogError(string.Format("sceneId id : {0} not exist", sceneId));
            return;
        }

        GameWorld.StartCoroutine(LoadImpl(sceneDeploy, finishAction));
    }

    private static IEnumerator LoadImpl(SceneDeploy sceneDeploy, Action<SceneBase> finishAction)
    {
        //加载场景前先预加载
        yield return MissionCache.DoPreload(sceneDeploy);
        
        var scenePath = sceneDeploy.path;
        if (string.IsNullOrEmpty(scenePath))
        {
            Dbg.LogError("ScenePath is null!!!");
            yield break;
        }

        Debug.Log("StartLoadScene:" + scenePath);
        
        var realSceneName = "";
        yield return XResource.LoadScene(scenePath, LoadSceneMode.Single, sceneName =>
        {
            realSceneName = sceneName;
        });

        //挂脚本
        var typeScene = GetSceneType(sceneDeploy);
        var sceneObject = new GameObject("GameScene");
        var scene = sceneObject.AddComponent(typeScene) as SceneBase;
        if (scene != null)
        {
            scene.SceneName = realSceneName;
            scene.SceneDeploy = sceneDeploy;
            yield return scene.Init();
            yield return XResource.DoGc();
            finishAction?.Invoke(scene);
        }
        else
        {
            finishAction?.Invoke(null);
        }
    }

    public static void OnSceneEnter(SceneBase scene)
    {
        if (ActiveScene != null)
        {
            ActiveScene.OnLeave();
        }
        ActiveScene = scene;
    }

    public static void OnSceneLeave(SceneBase scene)
    {
        ActiveScene = null;
    }
}