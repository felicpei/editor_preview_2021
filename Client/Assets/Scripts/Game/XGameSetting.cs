using System;
using UnityEngine;


//X项目用到的设置选项，以前有用的都尽量往这里挪
public class XGameSetting
{
    //分辨率
    public enum EnumResolution
    {
        Low = 0,
        Mid = 1,
        High = 2
    }

    private static SettingData _data;

    public static void Init()
    {
        _data = SystemDataMgr.Data.SettingData;
        _defaultScreenHeight = Screen.height;
        RefreshGameResolutions(XResolution);
    }
    
    public static EnumResolution XResolution
    {
        get => _data.Resolution;
        set
        {
            if (_data.Resolution != value)
            {
                _data.Resolution = value;
                _data.Save();
                
                RefreshGameResolutions(value);
            }
        }
    }

    //振动
    public static bool XShake
    {
        get => _data.Shake;
        set
        {
            if (_data.Shake != value)
            {
                _data.Shake = value;
                _data.Save();
            }
        }
    }

    public static float MusicVolume
    {
        set
        {
            if (Math.Abs(_data.MusicVolume - value) > 0.01f)
            {
                _data.MusicVolume = Math.Max(0f, value);
                _data.Save();
                Sound.GlobalMusicVolume = _data.MusicVolume;
            }
        }
        get => _data.MusicVolume;
    }

    public static float AudioVolume
    {
        set
        {
            if (Math.Abs(_data.AudioVolume - value) > 0.01f)
            {
                _data.AudioVolume = Math.Max(0f, value);
                _data.Save();
                Sound.GlobalAudioVolume = _data.AudioVolume;
            }
        }
        get => _data.AudioVolume;
    }
    
    public static int GetXResolution(EnumResolution xResolution)
    {
        switch (xResolution)
        {
            case EnumResolution.Low:
                return 240;
            case EnumResolution.Mid:
                return 640;
            case EnumResolution.High:
                return 1080;
        }

        return 720;
    }


    private static Resolution _originResolution;
    private static int _defaultScreenHeight;

    public static void RefreshGameResolutions(EnumResolution xResolution)
    {
        //webgl 不处理分辨率
        if (XPlatform.Platform is XPlatform.EPlatform.WebGL or XPlatform.EPlatform.Standalone)
        {
            return;
        }

        var origin = GetXResolution(xResolution);
        var designWidth = origin;
        var designHeight = (int)(designWidth / (Screen.currentResolution.width / (float)Screen.currentResolution.height));

        if (Screen.currentResolution.width == designWidth && Screen.currentResolution.height == designHeight)
        {
            return;
        }

        if (_originResolution.width == 0 && _originResolution.height == 0)
        {
            _originResolution = Screen.currentResolution;
        }

        Screen.SetResolution(designWidth, designHeight, true);
        Dbg.Log(string.Format("SetResolution: {0}x{1} dpi:{2}", designWidth, designHeight, Screen.dpi) + "DefaultScreenHeight:" + _defaultScreenHeight);
        Dbg.Log(string.Format("Screen: {0}x{1} dpi:{2}", Screen.width, Screen.height, Screen.dpi));
    }
}