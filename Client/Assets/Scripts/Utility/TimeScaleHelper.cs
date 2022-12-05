using System.Collections.Generic;
using UnityEngine;

public enum ETimeScaleFrom
{
    Pause,
    BulletTime,
    Test1,
    Test2,
}

public static class TimeScaleHelper
{
    private static readonly Dictionary<ETimeScaleFrom, float> TimeScaleData = new();
    
    public static void SetTimeScale(ETimeScaleFrom from, float value)
    {
        TimeScaleData[from] = value;
        CalcTimeScale();

    }

    public static void ClearTimeScale(ETimeScaleFrom from)
    {
        TimeScaleData[from] = 1f;
        CalcTimeScale();
    }

    private static void CalcTimeScale()
    {
        var timeScale = 1f;
        foreach (var kv in TimeScaleData)
        {
            timeScale *= kv.Value;
        }

        Time.timeScale = timeScale;
        GameEventCenter.Send(GameEvent.OnTimeScaleChanged, Time.timeScale);
    }

    public static float GetTimeScale()
    {
        return Time.timeScale;
    }

    public static void ResetTimeScale() 
    {
        TimeScaleData.Clear();
        Time.timeScale = 1;
    }
}
