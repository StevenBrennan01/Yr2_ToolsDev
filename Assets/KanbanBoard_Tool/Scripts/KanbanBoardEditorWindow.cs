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

        EditorUtility.SetDirty(kanbanData);

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

                taskListView.makeItem = () =>
                {
                    VisualElement container = new VisualElement();
                    TextField nameField = new TextField();
                    TextField descriptionField = new TextField();
                    EnumField stateDropdown = new EnumField(KanbanTaskState.ToDo); // Default value

                    nameField.name = "TaskNameField";
                    descriptionField.name = "TaskDescriptionField";
                    stateDropdown.name = "TaskStateDropdown";

                    container.Add(nameField);
                    container.Add(descriptionField);
                    container.Add(stateDropdown);

                    return container;
                };
                taskListView.bindItem = (element, index) =>
                {
                    KanbanTask task = kanbanData.Tasks[index];

                    TextField nameField = element.Q<TextField>("TaskNameField");
                    TextField descriptionField = element.Q<TextField>("TaskDescriptionField");
                    EnumField stateDropdown = element.Q<EnumField>("TaskStateDropdown");

                    // Bind data
                    nameField.value = task.taskTitle;
                    descriptionField.value = task.taskDescription;
                    stateDropdown.value = task.state;

                    nameField.RegisterValueChangedCallback(evt => task.taskTitle = evt.newValue);
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

    private void SaveData()
    {
        EditorUtility.SetDirty(kanbanData);
        AssetDatabase.SaveAssets();
    }
}