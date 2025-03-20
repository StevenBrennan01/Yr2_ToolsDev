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
        string dataAssetPath = "Assets/KanbanBoard_Tool/KanbanData/KanbanBoardDataManager.asset";

        kanbanData = AssetDatabase.LoadAssetAtPath<KanbanBoardDataManager>(dataAssetPath);

        if (kanbanData == null)
        {
            kanbanData = CreateInstance<KanbanBoardDataManager>();
            AssetDatabase.CreateAsset(kanbanData, dataAssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Created new KanbanBoardDataManager asset.");
        }

        EditorUtility.SetDirty(kanbanData);

        rootVisualElement.Clear();
        GenerateUI();
    }

    private void GenerateUI()
    {
        #region Importing UXML & StyleSheet

        // Importing in the UXML File
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/KanbanBoardData.uxml");
        if (visualTree != null)
        {
            VisualElement ui = visualTree.Instantiate();
            rootVisualElement.Add(ui);

            ListView taskListView = rootVisualElement.Q<ListView>("TaskListView");

            if (taskListView != null)
            {

                if (kanbanData.Tasks.Count == 0)
                {
                    Debug.Log("No tasks found within the Kanban Board Data Manager");
                }

                taskListView.itemsSource = kanbanData.Tasks;

                taskListView.makeItem = () =>
                {
                    VisualElement container = new VisualElement();
                    TextField titleField = new TextField();
                    TextField descriptionField = new TextField();
                    EnumField stateDropdown = new EnumField(KanbanTaskState.ToDo);

                    titleField.name = "TaskTitleField";
                    descriptionField.name = "TaskDescriptionField";
                    stateDropdown.name = "TaskStateDropdown";

                    container.Add(titleField);
                    container.Add(descriptionField);
                    container.Add(stateDropdown);

                    return container;
                };

                taskListView.bindItem = (element, index) =>
                {
                    KanbanTask task = kanbanData.Tasks[index];

                    TextField titleField = element.Q<TextField>("TaskTitleField");
                    TextField descriptionField = element.Q<TextField>("TaskDescriptionField");
                    EnumField stateDropdown = element.Q<EnumField>("TaskStateDropdown");

                    // Bind data
                    titleField.value = task.taskTitle;
                    descriptionField.value = task.taskDescription;
                    stateDropdown.value = task.state;

                    titleField.RegisterValueChangedCallback(evt => task.taskTitle = evt.newValue);
                    descriptionField.RegisterValueChangedCallback(evt => task.taskDescription = evt.newValue);
                    stateDropdown.RegisterValueChangedCallback(evt => task.state = (KanbanTaskState)evt.newValue);
                };

                taskListView.Rebuild();
                taskListView.RefreshItems();
            }
        }
        else
        {
            Debug.Log("UXML File not found, Check it exists and also check for correct path");
        }

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
    }

    private void SaveData()
    {
        EditorUtility.SetDirty(kanbanData);
        AssetDatabase.SaveAssets();
    }
}