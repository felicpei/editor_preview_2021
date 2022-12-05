using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(XUI_GridLayout))]
[CanEditMultipleObjects]
public class XUI_GridLayoutEditor : Editor
{
    private XUI_GridLayout _gridLayout;
    private SerializedProperty fitHeightProperty;
    private SerializedProperty cellSizeProperty;
    private SerializedProperty spaceProperty;

    private bool _fitHeight;
    private Vector2 _cellSize;
    private Vector2 _space;

    protected void OnEnable()
    {
        fitHeightProperty = serializedObject.FindProperty("FitHeight");
        cellSizeProperty = serializedObject.FindProperty("CellSize");
        spaceProperty = serializedObject.FindProperty("Space");

        RecordValue();
    }

    private void RecordValue()
    {
        _fitHeight = fitHeightProperty.boolValue;
        _cellSize = cellSizeProperty.vector2Value;
        _space = spaceProperty.vector2Value;
    }

    public override void OnInspectorGUI()
    {
        _gridLayout = target as XUI_GridLayout;
        if (!_gridLayout) return;

        serializedObject.Update();
        base.OnInspectorGUI();

        bool click = GUILayout.Button("sort!");

        if (ValueChange() || click)
        {
            DoDirty();
            RecordValue();
            _gridLayout.Sort();
        }
        
    }

    private bool ValueChange()
    {
        return _fitHeight != fitHeightProperty.boolValue ||
               _cellSize != cellSizeProperty.vector2Value ||
               _space != spaceProperty.vector2Value;
    }

    private void DoDirty()
    {
        if (_gridLayout == null)
        {
            return;
        }
        
        var prefabStage = PrefabStageUtility.GetPrefabStage(_gridLayout.gameObject);
        if (prefabStage != null)
        {
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
    }
}
