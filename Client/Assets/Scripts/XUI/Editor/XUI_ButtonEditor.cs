using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(XUI_Button), true)]
    [CanEditMultipleObjects]
    public class XUI_ButtonEditor : ButtonEditor
    {
        SerializedProperty _debugProperty;
        private XUI_Button _button;
        private bool _isEnable;
        private bool _bUseTween;
        private bool _useDisplayImage;

        protected override void OnEnable()
        {
            base.OnEnable();
            _debugProperty = serializedObject.FindProperty("isDebug");
        }

        public override void OnInspectorGUI()
        {
            _button = target as XUI_Button;
            if (_button == null) return;

            serializedObject.Update();

            if (GUILayout.Button("Format"))
            {
                XUI_Button btn = (XUI_Button)target;
                btn.FormatStyle();
            }

            EditorGUILayout.PropertyField(_debugProperty);

            _bUseTween = EditorGUILayout.Toggle("UseTween", _button.UseTween);
            if (_button.UseTween != _bUseTween)
            {
                _button.SetUseTween(_bUseTween);
                DoDirty();
            }

            _isEnable = EditorGUILayout.Toggle("isEnableButton", _button.isEnable);
            if (_button.isEnable != _isEnable)
            {
                _button.isEnable = _isEnable;
                DoDirty();
            }

            _useDisplayImage = EditorGUILayout.Toggle("UseDisplayImage", _button.UseDisplayImage);
            if (_button.UseDisplayImage != _useDisplayImage)
            {
                _button.UseDisplayImage = _useDisplayImage;
                DoDirty();
            }

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }

        private void DoDirty()
        {
            if (_button == null) return;
            var prefabStage = PrefabStageUtility.GetPrefabStage(_button.gameObject);
            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
        }
    }
}