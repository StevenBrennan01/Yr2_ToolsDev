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
            AssetDatabase.SaveAssets();
            Debug.Log("Created new KanbanBoardDataManager asset.");
        }

        // Save the data when the window is closed
        EditorUtility.SetDirty(kanbanData);

        rootVisualElement.Clear();
        GenerateUI();
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

            // Column Types
            VisualElement toDoColumn = rootVisualElement.Q<VisualElement>("ToDoColumn");
            VisualElement inProgressColumn = rootVisualElement.Q<VisualElement>("InProgressColumn");
            VisualElement toPolishColumn = rootVisualElement.Q<VisualElement>("ToPolishColumn");
            VisualElement finishedColumn = rootVisualElement.Q<VisualElement>("FinishedColumn");

            // Column Titles
            VisualElement firstColumnTitle = rootVisualElement.Q<VisualElement>("FirstColumnTitle");
            VisualElement secondColumnTitle = rootVisualElement.Q<VisualElement>("SecondColumnTitle");
            VisualElement thirdColumnTitle = rootVisualElement.Q<VisualElement>("ThirdColumnTitle");
            VisualElement fourthColumnTitle = rootVisualElement.Q<VisualElement>("FourthColumnTitle");

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
                            toDoColumn.Add(taskCard);
                            break;
                        case KanbanTaskState.InProgress:
                            inProgressColumn.Add(taskCard);
                            break;
                        case KanbanTaskState.ToPolish:
                            toPolishColumn.Add(taskCard);
                            break;
                        case KanbanTaskState.Finished:
                            finishedColumn.Add(taskCard);
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
        taskColour.RegisterValueChangedCallback(evt  => task.taskColour = evt.newValue);
        taskText.RegisterValueChangedCallback(evt => task.taskText = evt.newValue);
        stateDropdown.RegisterValueChangedCallback(evt => task.taskState = (KanbanTaskState)evt.newValue);

        // Register callbacks for drag and drop
        taskCard.RegisterCallback<PointerDownEvent>(evt => OnTaskPointerDown(evt, taskCard));
        taskCard.RegisterCallback<PointerMoveEvent>(evt => OnTaskPointerMove(evt, taskCard));
        taskCard.RegisterCallback<PointerUpEvent>(evt => OnTaskPointerUp(evt, taskCard));

        return taskCard;
    }

    private void OnTaskPointerDown(PointerDownEvent evt, VisualElement taskCard)
    {
        //implement logic for dragging the task card
    }

    private void OnTaskPointerMove(PointerMoveEvent evt, VisualElement taskCard)
    {
        //implement logic for moving the task card
    }

    private void OnTaskPointerUp(PointerUpEvent evt, VisualElement taskCard)
    {
        //implement logic for dropping the task card
    }

    private void SaveData()
    {
        EditorUtility.SetDirty(kanbanData);
        AssetDatabase.SaveAssets();
    }
}