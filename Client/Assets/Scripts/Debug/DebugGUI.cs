using System;
using UnityEngine;

namespace Script
{
    public class DebugGUI : MonoBehaviour
    {

        private float m_UpdateShowDeltaTime;
        private float m_FrameUpdate;
        private float m_FPS;

        private string m_fpsStr;
        private string m_memory;
        private string m_resolution;

        public static int MonsterCount;
        private string m_strMonsterCount;

        private void Awake()
        {
            
#if UNITY_ANDROID
    Application.targetFrameRate = 120;
#endif
        }

        private void Update()
        {
            m_FrameUpdate += 1;
            m_UpdateShowDeltaTime += Time.deltaTime;

            if (m_UpdateShowDeltaTime >= 1)
            {
                m_FPS = m_FrameUpdate / m_UpdateShowDeltaTime;
                m_UpdateShowDeltaTime = 0;
                m_FrameUpdate = 0;

                m_fpsStr = "Fps:" +  Mathf.Round(m_FPS);;
                m_memory = "Memory:" + Mathf.RoundToInt(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1000000f) + "M";
                m_resolution = "当前分辨率:" + Screen.width + "*" + Screen.height;
                m_strMonsterCount = "怪物数量:" + MonsterCount;
            }
        }

        private void OnGUI()
        {
            var x = 10;
            var y = 200;
            var w = 500;
            var h = 30;

            var split = 50;
            
            GUIHelper.GUIString(m_fpsStr, x, y, w, h, EGUIStyle.Red);
            y = y + split;
            
            GUIHelper.GUIString(m_memory, x, y, w, h, EGUIStyle.Red);
            y = y + split;

            GUIHelper.GUIString(m_resolution, x, y, w, h, EGUIStyle.Red);
            y = y + split;
            
            GUIHelper.GUIString(m_strMonsterCount, x, y, w, h, EGUIStyle.Red);
            y = y + split;
            
        }
    }
    
    public enum EGUIStyle
    {
        Red = 1,
        Green = 2,
        Normal = 3
    }

    public static class GUIHelper
    {
        private static GUIStyle _redStyle;
        private static GUIStyle _greenStyle;
        private static GUIStyle _whiteStyle;
    
        private static GUIStyle RedStyle => _redStyle ??= new GUIStyle { normal = { textColor = Color.red }, fontSize = 40 };
        private static GUIStyle GreenStyle => _greenStyle ??= new GUIStyle { normal = { textColor = Color.green }, fontSize = 40 };
        private static GUIStyle WhiteStyle => _whiteStyle ??= new GUIStyle { normal = { textColor = Color.white }, fontSize = 40 };
    
        public static void GUIString(string str, float x, float y, float width, float height, EGUIStyle style)
        {
            GUIStyle useStyle;
            switch (style)
            {
                case EGUIStyle.Red:
                    useStyle = RedStyle;
                    break;
                case EGUIStyle.Green:
                    useStyle = GreenStyle;
                    break;
                default:
                    useStyle = WhiteStyle;
                    break;
            }
        
            GUI.Label(new Rect(x, y, width, height), str, useStyle);
        }

        public static void GUIButton(string str, float x, float y, float width, float height, Action callBack)
        {
            if (GUI.Button(new Rect(x, y, width, height),str))
            {
                callBack.Invoke();
            }
        }
    }
}