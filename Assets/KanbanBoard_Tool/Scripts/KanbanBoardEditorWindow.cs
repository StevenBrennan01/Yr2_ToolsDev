using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class KanbanBoardEditorWindow : EditorWindow
{
    private KanbanBoardDataManager kanbanData;
    private List<VisualElement> taskColumns;

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
        
        GenerateWindowUI();
    }

    // Saves the data when the window is closed
    private void OnDisable()
    {
        MarkDirtyAndSave();
    }

    private void MarkDirtyAndSave()
    {
        EditorUtility.SetDirty(kanbanData);
        AssetDatabase.SaveAssets();
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

        SetUpElements();
    }

    private void SetUpElements() // refresh is refreshing every element even the ones that are not being edited, needs to be optimized
    {
        taskColumns = new List<VisualElement>();

        for (int i = 0; i < 4; i++)
        {
            AddTaskColumn($"Edit Column Title:{i + 1}");
        }

        // Add/Delete task buttons
        Button addTaskButton = rootVisualElement.Q<Button>("AddTaskButton");
        Button deleteTaskButton = rootVisualElement.Q<Button>("DeleteTaskButton");

        Slider AddExtraColumns = rootVisualElement.Q<Slider>("ColumnSlider");

        // Column Types (currently: todo, in progress, to polish, finished) 
        // *Look into allowing the user to add more columns*
        VisualElement boardEditorSlot = rootVisualElement.Q<VisualElement>("NewTaskBox");

        // Use this to only allow for one task card to be created at a time
        int NewTaskCount = 0;

        // Button for adding new task into the BoardEditor
        addTaskButton.RegisterCallback<ClickEvent>(evt =>
        {
            // Add a new task to the new task box (board editor)
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
            PopulateTaskCards(taskCard, newTask);
            boardEditorSlot.Add(taskCard);
        });

        // Button for deleting the last task in the BoardEditor
        // *Maybe try and make it so that the user can delete a selected task in the future*
        deleteTaskButton.RegisterCallback<ClickEvent>(evt =>
        {
            // Delete the last task in the new task box
            if (kanbanData.Tasks.Count > 0)
            {
                kanbanData.Tasks.RemoveAt(kanbanData.Tasks.Count - 1);
                boardEditorSlot.RemoveAt(boardEditorSlot.childCount - 1);
            }
        });
    }

    private void AddTaskColumn(string columnName)
    {
        VisualElement taskColumn = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskColumn.uxml").Instantiate();
        if (taskColumn != null)
        {
            taskColumn.name = columnName;
            rootVisualElement.Q<VisualElement>("ColumnContainer").Add(taskColumn);
            taskColumns.Add(taskColumn);
        }
        else
        {
            Debug.Log("TaskColumn.uxml not found, check it exists and also check for correct path");
        }
    }

    // GOING TO NEED THIS WORKING FOR ALL COLUMNS NOT JUST [0]
    private void PopulateTaskColumns()
    {
        TextField columnTitle = rootVisualElement.Q<TextField>(/*CollumnName String*/);

        columnTitle.value = kanbanData.ColumnTitles[0]; // Initialize the column title with the first column title

        columnTitle.RegisterValueChangedCallback(evt => DebounceAndSaveColumnTitles(() => kanbanData.ColumnTitles[0] = evt.newValue, columnTitle));
    }

    private void AddTaskCards()
    {
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
                PopulateTaskCards(taskCard, task);
                MarkDirtyAndSave();
            }
        }
        else
        {
            Debug.Log("Cannot find TaskCard.uxml file. Check the path and file name.");
        }
    }

    private VisualElement PopulateTaskCards(VisualElement taskCard, KanbanTask task)
    {
        // Loading individual cards into UXML depending on how many tasks exist

        TextField taskText = taskCard.Q<TextField>("TaskText");
        EnumField stateDropdown = taskCard.Q<EnumField>("TaskState");
        ColorField taskColour = taskCard.Q<ColorField>("TaskColor");

        // Initializing the TaskText and Colour
        taskText.value = task.taskText;
        taskColour.value = task.taskColour;
        stateDropdown.value = task.taskState;
        //stateDropdown.Init(/*initialise in the board editor*/);

        // Register callbacks for updating the task data (task, state, colour)
        taskText.RegisterValueChangedCallback(evt => DebounceAndSaveTaskCards(() => task.taskText = evt.newValue, taskCard, task)) ;
        taskColour.RegisterValueChangedCallback(evt => DebounceAndSaveTaskCards(() => task.taskColour = evt.newValue, taskCard, task));
        stateDropdown.RegisterValueChangedCallback(evt =>
        {
            DebounceAndSaveTaskCards(() => task.taskState = (KanbanTaskState)evt.newValue, taskCard, task);
        });

        // Register callbacks for drag and drop
        taskCard.RegisterCallback<PointerDownEvent>(evt => OnTaskPointerDown(evt, taskCard));
        taskCard.RegisterCallback<PointerMoveEvent>(evt => OnTaskPointerMove(evt, taskCard));
        taskCard.RegisterCallback<PointerUpEvent>(evt => OnTaskPointerUp(evt, taskCard));

        return taskCard;
    }

    private VisualElement draggedTaskCard;
    private Vector2 dragOffset;

    private void OnTaskPointerDown(PointerDownEvent evt, VisualElement taskCard)
    {
        draggedTaskCard = taskCard;
        dragOffset = evt.localPosition;
        taskCard.CaptureMouse(); // Capture mouse events for the task card
    }

    private void OnTaskPointerMove(PointerMoveEvent evt, VisualElement taskCard)
    {
        if (draggedTaskCard != null && taskCard.HasMouseCapture()) // Check if taskcard is captured by mouse on TaskPointerDown
        {
            Vector2 newCardPosition = evt.localPosition - dragOffset;
            taskCard.transform.position = newCardPosition;
        }
    }

    private void OnTaskPointerUp(PointerUpEvent evt, VisualElement taskCard)
    {
        //implement logic for dropping the task card
        Debug.Log("Task card dropped");
    }

    private void DebounceAndSaveColumnTitles(Action updateAction, TextField columnTitle)
    {
        updateAction.Invoke();
        DebounceUtility.Debounce(() =>
        {
            MarkDirtyAndSave();
        }, .5f);
    }

    private void DebounceAndSaveTaskCards(Action updateAction, VisualElement taskcard, KanbanTask task)
    {
        updateAction.Invoke();
        DebounceUtility.Debounce(() =>
        {
            MarkDirtyAndSave();
        }, .5f);
    }
}