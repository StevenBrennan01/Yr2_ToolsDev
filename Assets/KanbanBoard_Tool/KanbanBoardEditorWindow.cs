using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class KanbanBoardEditorWindow : EditorWindow
{
    private KanbanBoardDataManager kanbanData;

    [MenuItem("My Tools/Kanban Board")]
    public static void OpenWindow()
    {
        var window = GetWindow<KanbanBoardEditorWindow>("Kanban Board");
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

        GenerateUI();
    }

    private void GenerateUI()
    {
        rootVisualElement.Clear();

        var titleLabel = new Label("Workflow Tracker & Planner");
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        titleLabel.style.fontSize = 20;
        titleLabel.style.marginBottom = 10;
        rootVisualElement.Add(titleLabel);
    }
}