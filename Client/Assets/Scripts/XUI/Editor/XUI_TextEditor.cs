using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(XUI_Text), true)]
    [CanEditMultipleObjects]
    public class XUI_TextEditor : TextEditor
    {
        private XUI_Text text;
        private Texture2D texture2D;

        private bool _needLocation;
        private bool _bUnderLine;
        private static Font _defaultFont;
        protected override void OnEnable()
        {
            base.OnEnable();
            texture2D = new Texture2D(30, 20);
        }

        private void NewColorBtn(Color color)
        {
            int width = 30;
            float a = color.a;
            float a_w = (int)(a * width);
            for (int i = 0; i < width; i++)
            {
                for (int k = 0; k < 5; k++)
                {
                    //伪装成alpha条
                    if (i <= a_w)
                    {
                        texture2D.SetPixel(i, k, Color.white);
                    }

                    if (i > a_w)
                    {
                        texture2D.SetPixel(i, k, Color.black);
                    }
                }

                for (int j = 5; j < 20; j++)
                {
                    texture2D.SetPixel(i, j, color);
                }
            }
            texture2D.Apply();

            if (GUILayout.Button(texture2D, GUILayout.MaxWidth(30))) ClickColor();
        }

        public override void OnInspectorGUI()
        {
            text = target as XUI_Text;
            if (_defaultFont == null)
            {
                _defaultFont = XResource.LoadEditorAsset("UI/Fonts/msyhbd.ttf") as Font;
            }

            serializedObject.Update();

            if (text.font != _defaultFont)
            {
                text.font = _defaultFont;
                DoDirty();
            }

            EditorGUILayout.Space();
            GUILayout.BeginVertical();

            if (GUILayout.Button("AddOutLine"))
            {
                text.AddOutLine();
                DoDirty();
            }
            if (GUILayout.Button("RemoveOutLine"))
            {
                text.RemoveOutLine();
                DoDirty();
            }
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            if (text != null)
            {
                _needLocation = EditorGUILayout.Toggle("本地化", text.needLocalization);
                if (_needLocation != text.needLocalization)
                {
                    text.needLocalization = _needLocation;
                    DoDirty();
                }

                _bUnderLine = EditorGUILayout.Toggle("UnderLine", text.underLine);
                if (_bUnderLine != text.underLine)
                {
                    text.underLine = _bUnderLine;
                    DoDirty();
                }

                if (text.underLine)
                    text.SyncText();
            }

            base.OnInspectorGUI();
        }

        private void ClickColor()
        {
            text.color = texture2D.GetPixel(0, 5);
            DoDirty();
        }


        private void DoDirty()
        {
            if (text == null) return;
            var prefabStage = PrefabStageUtility.GetPrefabStage(text.gameObject);
            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
        }
    }
}
