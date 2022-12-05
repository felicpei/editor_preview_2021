using System.Collections;
using LitJson;
using UnityEngine;


public class XPlatform
{
    public enum EPlatform
    {
        UnityEditor = 1,
        UnityEditor_AB = 2,
        Standalone = 3,
        Android = 4,
        Ios = 5,
        WebGL = 6
    }

    public static bool InEditor => Platform == EPlatform.UnityEditor_AB || Platform == EPlatform.UnityEditor;
    public static bool InMobile => Platform == EPlatform.Ios || Platform == EPlatform.Android;
    public static EPlatform Platform = EPlatform.UnityEditor;
    private static bool _bInit;
    
    public static IEnumerator Init(bool useEditorAssetBundle)
    {
        if (_bInit)
        {
            yield break;
        }
        _bInit = true;

        Dbg.IsDebugBuild = Debug.isDebugBuild;
        
#if UNITY_EDITOR
        Platform = useEditorAssetBundle ? EPlatform.UnityEditor_AB : EPlatform.UnityEditor;

#elif UNITY_ANDROID
        Platform = EPlatform.Android;
        Application.targetFrameRate = 120;
#elif UNITY_IOS
        Platform = EPlatform.Ios;
        Application.targetFrameRate = 120;
#elif UNITY_STANDALONE
        Platform = EPlatform.Standalone;
        Application.targetFrameRate = 144;
#elif UNITY_WEBGL
        Platform = EPlatform.WebGL;
        Application.targetFrameRate = 120;
#endif
    }

    public static string GetPlatform()
    {
        return Platform.ToString();
    }

    private static string _deviceId;

    private static string GetDeviceId()
    {
        if (!string.IsNullOrEmpty(_deviceId))
        {
            return _deviceId;
        }
        
        var deviceID = LocalStorage.Read(LocalStorage.Key.DeviceId);
        if (!string.IsNullOrEmpty(deviceID))
        {
            return deviceID;
        }

        deviceID = SystemInfo.deviceUniqueIdentifier;
        if (string.IsNullOrEmpty(deviceID))
        {
            deviceID = "unkown";
        }

        if (deviceID.TrimEnd(new[] { '0' }) == "imei_")
        {
            deviceID = SystemInfo.deviceUniqueIdentifier;
        }

        if (deviceID.TrimEnd('0', ':') == "mac_")
        {
            deviceID = SystemInfo.deviceUniqueIdentifier;
        }

        LocalStorage.Write(LocalStorage.Key.DeviceId, deviceID);
        _deviceId = deviceID;
        return deviceID;
    }
}