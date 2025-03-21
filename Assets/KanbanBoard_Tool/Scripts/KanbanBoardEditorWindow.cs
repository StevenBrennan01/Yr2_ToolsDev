using UnityEditor;
using UnityEditor.UIElements;
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
            Debug.Log("Created new KanbanBoardDataManager asset.");
        }

        rootVisualElement.Clear();
        GenerateUI();
    }

    // Save the data when the window is closed
    private void OnDisable()
    {
        SaveData();
    }

    private void GenerateUI()
    {
        #region Importing UXML & StyleSheet

        // Importing in the UXML File
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/KanbanBoard.uxml");
        if (visualTree != null)
        {
            VisualElement ui = visualTree.Instantiate();
            rootVisualElement.Add(ui);

            // Column Types (todo, in progress, to polish, finished) 
            // Look into allowing the user to add more columns
            VisualElement Column1 = rootVisualElement.Q<VisualElement>("Column1");
            VisualElement Column2 = rootVisualElement.Q<VisualElement>("Column2");
            VisualElement Column3 = rootVisualElement.Q<VisualElement>("Column3");
            VisualElement Column4 = rootVisualElement.Q<VisualElement>("Column4");

            // Try make it so that tasks begin in the this column
            // And also attempt custom amount of columns
            VisualElement newTaskBox = rootVisualElement.Q<VisualElement>("NewTaskBox");

            // Buttons to add and delete tasks
            Button addTaskButton = rootVisualElement.Q<Button>("AddTaskButton");
            Button deleteTaskButton = rootVisualElement.Q<Button>("DeleteTaskButton");

            // Column Titles (Text Fields)
            TextField column1Title = rootVisualElement.Q<TextField>("FirstColumnTitle");
            TextField column2Title = rootVisualElement.Q<TextField>("SecondColumnTitle");
            TextField column3Title = rootVisualElement.Q<TextField>("ThirdColumnTitle");
            TextField column4Title = rootVisualElement.Q<TextField>("FourthColumnTitle");

            column1Title.value = kanbanData.column1Title;
            column2Title.value = kanbanData.column2Title;
            column3Title.value = kanbanData.column3Title;
            column4Title.value = kanbanData.column4Title;

            column1Title.RegisterValueChangedCallback(evt =>
            {
                kanbanData.column1Title = evt.newValue;
                SaveData();
            });

            column2Title.RegisterValueChangedCallback(evt =>
            {
                kanbanData.column2Title = evt.newValue;
                SaveData();
            });

            column3Title.RegisterValueChangedCallback(evt =>
            {
                kanbanData.column3Title = evt.newValue;
                SaveData();
            });

            column4Title.RegisterValueChangedCallback(evt =>
            {
                kanbanData.column4Title = evt.newValue;
                SaveData();
            });

            if (kanbanData.Tasks.Count == 0)
            {
                Debug.Log("No tasks found in the data manager.");
            }

            var taskCardTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskCard.uxml");
            if (taskCardTemplate != null)
            {
                // Generating Task Cards
                foreach (var task in kanbanData.Tasks)
                {
                    // Instantiating a card for each task that exists
                    VisualElement taskCard = taskCardTemplate.Instantiate();

                    if (taskCard == null)
                    {
                        Debug.Log("Instantiating TaskCard failed");
                        continue;
                    }

                    CreateTaskCard(taskCard, task);

                    switch (task.taskState)
                    {
                        case KanbanTaskState.ToDo:
                            Column1.Add(taskCard);
                            break;
                        case KanbanTaskState.InProgress:
                            Column2.Add(taskCard);
                            break;
                        case KanbanTaskState.ToPolish:
                            Column3.Add(taskCard);
                            break;
                        case KanbanTaskState.Finished:
                            Column4.Add(taskCard);
                            break;
                        case KanbanTaskState.NewTask:
                            newTaskBox.Add(taskCard);
                            break;
                    }
                }
            }
            else
            {
                Debug.Log("Cannot find TaskCard.uxml file. Check the path and file name.");
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

    private VisualElement CreateTaskCard(VisualElement taskCard ,KanbanTask task)
    {
        // Loading individual cards into UXML depending on how many tasks exist

        TextField taskText = taskCard.Q<TextField>("TaskText");
        EnumField stateDropdown = taskCard.Q<EnumField>("TaskState");
        ColorField taskColour = taskCard.Q<ColorField>("TaskColor");
        
        // Initializing the TaskText and Colour
        taskText.value = task.taskText;
        taskColour.value = task.taskColour;

        // Initialize the dropdown with this state as a default
        stateDropdown.Init(KanbanTaskState.ToDo);
        stateDropdown.value = task.taskState;

        // Register callbacks for updating the task data (task, state, colour)
        taskText.RegisterValueChangedCallback(evt => task.taskText = evt.newValue);
        stateDropdown.RegisterValueChangedCallback(evt => task.taskState = (KanbanTaskState)evt.newValue);
        taskColour.RegisterValueChangedCallback(evt => task.taskColour = evt.newValue);

        // Register callbacks for drag and drop
        taskCard.RegisterCallback<PointerDownEvent>(evt => OnTaskPointerDown(evt, taskCard));
        taskCard.RegisterCallback<PointerMoveEvent>(evt => OnTaskPointerMove(evt, taskCard));
        taskCard.RegisterCallback<PointerUpEvent>(evt => OnTaskPointerUp(evt, taskCard));

        return taskCard;
    }

    private void OnTaskPointerDown(PointerDownEvent evt, VisualElement taskCard)
    {
        //implement logic for dragging the task card
        Debug.Log("Task card clicked");
    }

    private void OnTaskPointerMove(PointerMoveEvent evt, VisualElement taskCard)
    {
        //implement logic for moving the task card
        Debug.Log("Task card is moving");
    }

    private void OnTaskPointerUp(PointerUpEvent evt, VisualElement taskCard)
    {
        //implement logic for dropping the task card
        Debug.Log("Task card dropped");
    }
    private void MarkDataDirty()
    {
        EditorUtility.SetDirty(kanbanData);
    }

    private void SaveData()
    {
        EditorUtility.SetDirty(kanbanData);
        AssetDatabase.SaveAssets();
    }
}