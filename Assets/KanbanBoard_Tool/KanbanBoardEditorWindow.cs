using UnityEditor;
using UnityEngine;

public class KanbanBoardEditorWindow : EditorWindow
{
    private KanbanBoardDataManager kanbanData;

    [MenuItem("My Tools/Kanban Board")]
    public static void OpenWindow()
    {
        var window = GetWindow<KanbanBoardEditorWindow>("Workflow Tracker & Planner");
        window.minSize = new Vector2(1000, 500);
    }

    private void OnEnable()
    {
        kanbanData = AssetDatabase.LoadAssetAtPath<KanbanBoardDataManager>("Assets/KanbanBoard_Tool/KanbanBoardDataManager.asset");

        if (kanbanData == null)
        {
            kanbanData = CreateInstance<KanbanBoardDataManager>();
            AssetDatabase.CreateAsset(kanbanData, "Assets/KanbanBoard_Tool/KanbanBoardDataManager.asset");
            AssetDatabase.SaveAssets();
        }

        //GenerateUI();
    }
}