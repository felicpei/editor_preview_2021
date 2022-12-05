
using System;
using UnityEngine;

[Flags]
public enum EPauseFrom
{
    None = 0,
}

public static class PauseHelper
{
    private static EPauseFrom _stateFrom = EPauseFrom.None;

    public static void SetPauseState(EPauseFrom state, bool bPause)
    {
        if (bPause)
        {
            _stateFrom |= state;
        }
        else
        {
            _stateFrom &= ~state;
        }
    }

    public static bool InPause => _stateFrom != EPauseFrom.None;

    public static void Revert()
    {
        _stateFrom = EPauseFrom.None;
        GameEventCenter.Send(GameEvent.OnGameContinue);
        TimeScaleHelper.ClearTimeScale(ETimeScaleFrom.Pause);
    }

    public static void PauseGame(EPauseFrom from)
    {
        Debug.Log("PauseGame:" + from);
        SetPauseState(from, true);
        if(_stateFrom != EPauseFrom.None)
        {
            CameraHelper.ClearShake();
            TimeScaleHelper.SetTimeScale(ETimeScaleFrom.Pause, 0f);
            GameEventCenter.Send(GameEvent.OnGamePause);
        }
    }

    public static void DoContinueGame(EPauseFrom from)
    {
        Debug.Log("DoContinueGame:" + from + " " + _stateFrom);
        
        SetPauseState(from, false);

        if (_stateFrom == EPauseFrom.None) 
        {
            GameEventCenter.Send(GameEvent.OnGameContinue);
            TimeScaleHelper.ClearTimeScale(ETimeScaleFrom.Pause);
        }
    }
}
