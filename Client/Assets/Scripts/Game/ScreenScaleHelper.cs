using System;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class ScreenScaleHelper
{
    public static void Init()
    {
    }


    public static void AdjustCamera()
    {
        
        var safeArea = Screen.safeArea;
        var widthRadio =  safeArea.width / Screen.width;
        var heightRadio = safeArea.height / Screen.height;
        var startX = safeArea.x / Screen.width / 2;
        var startY = safeArea.y / Screen.height / 2;
        
        Debug.Log("Screen.safeArea:" + Screen.safeArea + "ScreenWidth:" + Screen.width + " ScreenHeight:" + Screen.height);
        XUI_Manager.RefreshCanvasSize(widthRadio, heightRadio, startX, startY);
    }
}