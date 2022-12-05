using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(XUI_RawImage), true)]
    [CanEditMultipleObjects]
    public class XUI_RawImageEditor : RawImageEditor
    {
        SerializedProperty UVAnimation;
        SerializedProperty UVSpeedX;
        SerializedProperty UVSpeedY;

        protected override void OnEnable()
        {
            base.OnEnable();
            UVAnimation = serializedObject.FindProperty("UVAnimation");
            UVSpeedX = serializedObject.FindProperty("SpeedX");
            UVSpeedY = serializedObject.FindProperty("SpeedY");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(UVAnimation);
            EditorGUILayout.PropertyField(UVSpeedX);
            EditorGUILayout.PropertyField(UVSpeedY);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
