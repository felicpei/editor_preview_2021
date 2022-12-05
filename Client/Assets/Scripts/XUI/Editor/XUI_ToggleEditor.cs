using UnityEditor.SceneManagement;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(XUI_Toggle), true)]
    [CanEditMultipleObjects]
    public class XUI_ToggleEditor : ToggleEditor
    {
        private XUI_Toggle _toggle;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _toggle = target as XUI_Toggle;
            if (_toggle != null)
            {
                var old1 = _toggle.IsDebug;
                var old3 = _toggle.TextColorChange;

                _toggle.IsDebug = EditorGUILayout.Toggle("IsDebug", _toggle.IsDebug);
                _toggle.TextColorChange = EditorGUILayout.Toggle("TextColorChange", _toggle.TextColorChange);

                if (old1 != _toggle.IsDebug || old3 != _toggle.TextColorChange)
                {
                    DoDirty();
                }
            }
        }

        private void DoDirty()
        {
            if (_toggle == null)
            {
                return;
            }
            var prefabStage = PrefabStageUtility.GetPrefabStage(_toggle.gameObject);
            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
        }
    }
}