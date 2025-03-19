using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class KanbanBoardEditorWindow : EditorWindow
{
    private KanbanBoardDataManager kanbanData;

    [MenuItem("My Tools/Custom Kanban Board")]
    public static void OpenWindow()
    {
        var window = GetWindow<KanbanBoardEditorWindow>("Kanban Board");
        window.minSize = new Vector2(1000, 500);
    }

    private void OnEnable()
    {
        kanbanData = AssetDatabase.LoadAssetAtPath<KanbanBoardDataManager>("Assets/KanbanBoard_Tool/Scripts/KanbanBoardDataManager.asset");

        if (kanbanData == null)
        {
            kanbanData = CreateInstance<KanbanBoardDataManager>();
            AssetDatabase.CreateAsset(kanbanData, "Assets/KanbanBoard_Tool/KanbanData/KanbanBoardDataManager.asset");
            AssetDatabase.SaveAssets();
        }

        rootVisualElement.Clear();
        GenerateUI();
    }

    private void GenerateUI()
    {
        #region Importing UXML & StyleSheet

        #region VisualElement Importing
        // Importing in the UXML File
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/KanbanBoardData.uxml");
        if (visualTree != null)
        {
            VisualElement ui = visualTree.Instantiate();
            rootVisualElement.Add(ui);

            ListView taskListView = rootVisualElement.Q<ListView>("TaskListView");
            if (taskListView != null)
            {
                taskListView.itemsSource = kanbanData.Tasks;
                taskListView.makeItem = () => new Label();
                taskListView.bindItem = (element, index) =>
                {
                    Label label = element as Label;
                    label.text = kanbanData.Tasks[index].taskTitle;
                };

                taskListView.Rebuild();
            }
        }
        else
        {
            Debug.Log("UXML File not found, Check it exists and also check for correct path");
        }
        #endregion

        #region StyleSheet Importing
        // Importing in the StyleSheet
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/KanbanBoard_Tool/Window_UI/KanbanBoard.uss");
        if (styleSheet != null)
        {
            rootVisualElement.styleSheets.Add(styleSheet);
        }
        else
        {
            Debug.Log("StyleSheet not found, Check it exists and also check for correct path");
        }
        #endregion

        #endregion
    }
}