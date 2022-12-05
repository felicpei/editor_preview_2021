using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(XUI_Image), true)]
    [CanEditMultipleObjects]
    public class XUI_ImageEditor : ImageEditor
    {
        private XUI_Image _image;
        private Texture2D _texture2D;

        protected override void OnEnable()
        {
            base.OnEnable();
            _texture2D = new Texture2D(30, 20);
        }

        public override void OnInspectorGUI()
        {
            _image = target as XUI_Image;
            serializedObject.Update();

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        public void ClickColor()
        {
            _image.color = _texture2D.GetPixel(0, 5);
            DoDirty();
        }

        private void DoDirty()
        {
            if (_image == null)
            {
                return;
            }

            var prefabStage = PrefabStageUtility.GetPrefabStage(_image.gameObject);
            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
        }
    }
}