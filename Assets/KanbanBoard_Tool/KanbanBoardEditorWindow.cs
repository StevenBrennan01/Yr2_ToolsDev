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
        // Importing in the UXML File
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/KanbanBoardData.uxml");
        if (visualTree != null)
        {
            VisualElement ui = visualTree.Instantiate();
            rootVisualElement.Add(ui);
        }
        else
        {
            Debug.Log("UXML File not found, Check it exists and also check for correct path in code");
        }

        // Importing in the StyleSheet
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/KanbanBoard_Tool/Window_UI/KanbanBoard.uss");
        if (styleSheet != null)
        {
            rootVisualElement.styleSheets.Add(styleSheet);
        }
        else
        {
            Debug.Log("StyleSheet not found, Check it exists and also check for correct path in code");
        }

        // VVV This is just manually adding a title through code VVV
        //rootVisualElement.Clear();

        //var titleLabel = new Label("Workflow Tracker & Planner");
        //titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        //titleLabel.style.fontSize = 20;
        //titleLabel.style.marginBottom = 10;
        //rootVisualElement.Add(titleLabel);
    }
}