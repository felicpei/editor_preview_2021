using System;
using System.Collections;
using Script;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameInit : MonoBehaviour
{
    public Camera MainCamera;
    
    private void Awake()
    {
        //ecs editor静态变量不清空，特殊处理
#if UNITY_EDITOR
        FirstStart.Inited = false;        
#endif
        DontDestroyOnLoad(gameObject);
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        gameObject.AddComponentIfNotExists<DebugGUI>();
        yield return FirstStart.Init(true);
        
        //初始化全局协程
        GameWorld.StartCoroutine = StartCoroutine;
        
        //camera
        CameraHelper.Init(MainCamera);
        
        //一些必要的初始化过程
        XGameSetting.Init();
        XUI_Manager.Init();
        Sound.Init();
        Random.InitState(DateTime.Now.Second);
        
        //start game
        GameWorld.OnGameStart();
    }

    private void Update()
    {
        GameWorld.Update();
    }

    private void LateUpdate()
    {
        GameWorld.LateUpdate();
    }
    
    private void OnDestroy()
    {
        GameWorld.OnDestroy();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        GameWorld.OnApplicationPause(pauseStatus);
    }

    private void OnApplicationQuit()
    {
        GameWorld.OnApplicationQuit();
    }

    private void OnApplicationFocus(bool focusStatus)
    {
        GameWorld.OnApplicationFocus(focusStatus);
    }
}