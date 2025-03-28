using System;
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
        GenerateWindowUI();
    }

    // Saves the data when the window is closed
    private void OnDisable()
    {
        MarkDirtyAndSave();
    }

    private void GenerateWindowUI()
    {
        #region Importing UXML & StyleSheet

        // Importing in the UXML File
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/KanbanBoard.uxml");
        if (visualTree != null)
        {
            VisualElement ui = visualTree.Instantiate();
            rootVisualElement.Add(ui);
        }
        else
        {
            Debug.Log("UXML File not found, Check it exists and also check for correct path");
        }

        // Importing in the StyleSheet
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/KanbanBoard_Tool/Window_UI/KanbanBoardStyling.uss");
        if (styleSheet != null)
        {
            rootVisualElement.styleSheets.Add(styleSheet);
        }
        else
        {
            Debug.Log("StyleSheet not found, Check it exists and also check for correct path");
        }

        #endregion

        CreateNewTaskCard();
    }

    private void CreateNewTaskCard()
    {
        VisualElement newTaskBox = rootVisualElement.Q<VisualElement>("NewTaskBox");

        // Add/Delete task buttons
        Button addTaskButton = rootVisualElement.Q<Button>("AddTaskButton");
        Button deleteTaskButton = rootVisualElement.Q<Button>("DeleteTaskButton");

        // Column Types (currently: todo, in progress, to polish, finished) 
        // *Look into allowing the user to add more columns*
        VisualElement Column1 = rootVisualElement.Q<VisualElement>("Column1");
        VisualElement Column2 = rootVisualElement.Q<VisualElement>("Column2");
        VisualElement Column3 = rootVisualElement.Q<VisualElement>("Column3");
        VisualElement Column4 = rootVisualElement.Q<VisualElement>("Column4");

        // Column Titles
        TextField column1Title = rootVisualElement.Q<TextField>("FirstColumnTitle");
        TextField column2Title = rootVisualElement.Q<TextField>("SecondColumnTitle");
        TextField column3Title = rootVisualElement.Q<TextField>("ThirdColumnTitle");
        TextField column4Title = rootVisualElement.Q<TextField>("FourthColumnTitle");

        column1Title.value = kanbanData.column1Title;
        column2Title.value = kanbanData.column2Title;
        column3Title.value = kanbanData.column3Title;
        column4Title.value = kanbanData.column4Title;

        // Adding new task into the BoardEditor
        addTaskButton.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log("Instantiating new task card");

            // Add a new task to the new task box
            KanbanTask newTask = new KanbanTask();
            kanbanData.Tasks.Add(newTask);

            // Instantiating a card for the new task
            VisualElement taskCard = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskCard.uxml").Instantiate();

            if (taskCard == null)
            {
                Debug.Log("Instantiating TaskCard failed");
                return;
            }

            // THIS IS GENERATING A FRESH TASK CARD FOR THE NEW TASK
            PopulateTaskCard(taskCard, newTask);
            newTaskBox.Add(taskCard);
        });

        // Deleting the last task in the BoardEditor
        // *Maybe try and make it so that the user can delete a selected task in the future*
        deleteTaskButton.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log("Deleting task card");

            // Delete the last task in the new task box
            if (kanbanData.Tasks.Count > 0)
            {
                kanbanData.Tasks.RemoveAt(kanbanData.Tasks.Count - 1);
                newTaskBox.RemoveAt(newTaskBox.childCount - 1);
            }
        });

        column1Title.RegisterValueChangedCallback(evt =>
        {
            kanbanData.column1Title = evt.newValue;
            MarkDirtyAndSave();
        });

        column2Title.RegisterValueChangedCallback(evt =>
        {
            kanbanData.column2Title = evt.newValue;
            MarkDirtyAndSave();
        });

        column3Title.RegisterValueChangedCallback(evt =>
        {
            kanbanData.column3Title = evt.newValue;
            MarkDirtyAndSave();
        });

        column4Title.RegisterValueChangedCallback(evt =>
        {
            kanbanData.column4Title = evt.newValue;
            MarkDirtyAndSave();
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

                // THIS IS POPULATING THE GENERATED NEW TASK CARD WITH THE TASK DATA
                PopulateTaskCard(taskCard, task);

                // MOVING THE TASK CARDS INTO THE CORRECT STATE COLUMNS
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
                    case KanbanTaskState.BoardEditor:
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

    private VisualElement PopulateTaskCard(VisualElement taskCard, KanbanTask task)
    {
        // Loading individual cards into UXML depending on how many tasks exist

        TextField taskText = taskCard.Q<TextField>("TaskText");
        EnumField stateDropdown = taskCard.Q<EnumField>("TaskState");
        ColorField taskColour = taskCard.Q<ColorField>("TaskColor");

        // Initializing the TaskText and Colour
        taskText.value = task.taskText;
        taskColour.value = task.taskColour;

        // Initialize the dropdown with this state as a default
        stateDropdown.Init(KanbanTaskState.BoardEditor);
        stateDropdown.value = task.taskState;

        // Register callbacks for updating the task data (task, state, colour)
        taskText.RegisterValueChangedCallback(evt => DebounceSaveAndRefresh(() => task.taskText = evt.newValue));
        stateDropdown.RegisterValueChangedCallback(evt => DebounceSaveAndRefresh(() => task.taskState = (KanbanTaskState)evt.newValue));
        taskColour.RegisterValueChangedCallback(evt => DebounceSaveAndRefresh(() => task.taskColour = evt.newValue));

        // Register callbacks for drag and drop
        taskCard.RegisterCallback<PointerDownEvent>(evt => OnTaskPointerDown(evt, taskCard));
        taskCard.RegisterCallback<PointerMoveEvent>(evt => OnTaskPointerMove(evt, taskCard));
        taskCard.RegisterCallback<PointerUpEvent>(evt => OnTaskPointerUp(evt, taskCard));

        MarkDirtyAndSave();
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

    private void DebounceSaveAndRefresh(Action updateAction)
    {
        updateAction.Invoke();

        // i think this is slowing the window refresh because it's saving and refreshing at the same time
        DebounceUtility.Debounce(() => { MarkDirtyAndSave(); RefreshWindow(); }, 0.5f);
    }

    private void RefreshWindow()
    {
        // Refresh the window to show the changes

        // IS THERE A WAY TO REFRESH A NEW INSTANCE OF THE WINDOW WITHOUT DUPLICATING AND CLEARING THE ROOT VISUAL ELEMENT?
        rootVisualElement.Clear();
        GenerateWindowUI();
    }

    private void MarkDirtyAndSave()
    {
        EditorUtility.SetDirty(kanbanData);
        AssetDatabase.SaveAssets();
    }
}