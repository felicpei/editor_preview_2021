using System.Collections;
using UI;
using UnityEngine;

public static class GameWorld
{
    public delegate Coroutine StartCoroutineFunc(IEnumerator routine);
    public static StartCoroutineFunc StartCoroutine;

    public static bool FocusStatus { private set; get; }
    public static bool PauseStatus { private set; get; }



    public static void OnGameStart()
    {
        //启动游戏
        //进入MainCity
        SceneLoader.LoadMainCity(() =>
        {
        });
    }
    
    
    //切换场景清理资源
    public static void OnSceneLeave()
    {
        PauseHelper.Revert();
        TimeScaleHelper.ResetTimeScale();
        XUI_Manager.Clear();
        Sound.ClearAll();
        AtlasLoader.ClearCache();
    }
    
    public static void LateUpdate()
    {
    }

    public static void OnDestroy()
    {
    }

    public static void Update()
    {
        XUI_Manager.OnUpdate();
    }

    public static void OnApplicationFocus(bool statusParam)
    {
        FocusStatus = statusParam;
    }
    
    public static void OnApplicationPause(bool statusParam)
    {
        PauseStatus = statusParam;
    }

    public static void OnApplicationQuit()
    {
        XUI_Manager.OnApplicationQuit();
    }

    public static void Quit()
    {
        Application.Quit();
    }
}
