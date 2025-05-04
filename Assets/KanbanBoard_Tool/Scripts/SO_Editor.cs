using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(KanbanBoardDataManager))]
public class SO_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        KanbanBoardDataManager kanbanBoardDataManager = (KanbanBoardDataManager)target;
        if (GUILayout.Button("Reset Kanban Data"))
        {
            kanbanBoardDataManager.ResetKanbanData();
            EditorUtility.SetDirty(kanbanBoardDataManager);
        }
    }
}
