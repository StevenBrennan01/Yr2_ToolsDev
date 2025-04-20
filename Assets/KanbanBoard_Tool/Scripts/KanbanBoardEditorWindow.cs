using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        InitColumns();
        LoadSavedTaskData();
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

        InitBaseUiElements();
    }

    private void InitBaseUiElements()
    {
        taskColumns = new List<VisualElement>();

        //int initialColumnCount = 4; // Initial number of columns

        //while (kanbanData.Columns.Count < initialColumnCount)
        //{
        //    kanbanData.Columns.Add(new ColumnData {columnTitle = $"Edit Column Title: {initialColumnCount + 1}"});
        //}

        if (kanbanData.Columns.Count == 0)
        {
            // Initialize default columns if none exist in the ScriptableObject
            for (int i = 0; i < 4; i++)
            {
                kanbanData.Columns.Add(new ColumnData { columnTitle = $"Edit Column Title: {i + 1}" });
            }
            MarkDirtyAndSave();
        }

        foreach (var columnData in kanbanData.Columns)
        {
            // Load the column data into the UI
            LoadSavedColumnData(columnData);
        }

        VisualElement boardEditorSlot = rootVisualElement.Q<VisualElement>("NewTaskBox");

        // Add/Delete task buttons
        Button addTaskButton = rootVisualElement.Q<Button>("AddTaskButton");
        Button deleteTaskButton = rootVisualElement.Q<Button>("DeleteTaskButton");

        VisualElement columnContainer = rootVisualElement.Q<VisualElement>("ColumnContainer");
        SliderInt extraColumnSlider = rootVisualElement.Q<SliderInt>("ExtraColumnSlider");

        // Use this to only allow for one task card to be created at a time
        //int NewTaskCount = 0;

        extraColumnSlider.RegisterValueChangedCallback(evt =>
        {
            int sliderValue = (int)evt.newValue;
            int totalColumns = 4 + sliderValue;

            while (taskColumns.Count < totalColumns)
            {
                int columnIndex = taskColumns.Count + 1;
                string newColumnTitle = "Edit Column Title: ";
                LoadSavedColumnData(kanbanData.Columns[columnIndex]);
                kanbanData.ColumnTitles.Add(newColumnTitle); // Persist new column title
            }

            while (taskColumns.Count > totalColumns)
            {
                VisualElement lastColumn = taskColumns[taskColumns.Count - 1];
                columnContainer.Remove(lastColumn);
                taskColumns.RemoveAt(taskColumns.Count - 1);
            }

            MarkDirtyAndSave();
        });

        // Button for adding new task into the BoardEditor
        addTaskButton.RegisterCallback<ClickEvent>(evt =>
        {
            // Add a new task to the new task box (board editor)
            TaskData newTask = new TaskData();
            kanbanData.Tasks.Add(newTask);

            // Instantiating a card for the new task
            VisualElement taskCard = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskCard.uxml").Instantiate();

            if (taskCard == null)
            {
                Debug.Log("Instantiating TaskCard failed");
                return;
            }

            // THIS IS GENERATING A FRESH TASK CARD FOR THE NEW TASK
            InitTaskCards(taskCard, newTask);
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

    //private void InitColumns()
    //{
    //    for (int i = 0; i < kanbanData.ColumnTitles.Count; i++)
    //    {
    //        if (i >= taskColumns.Count)
    //        {
    //            Debug.Log("Column not found, check the number of columns in the kanbanData");
    //            continue;
    //        }

    //        VisualElement taskColumn = taskColumns[i];
    //        TextField columnTitle = taskColumn.Q<TextField>("ColumnTitle");

    //        columnTitle.value = kanbanData.ColumnTitles[i];

    //        int columnIndex = i; // Capture the current index for the callback
    //        columnTitle.RegisterValueChangedCallback(evt =>
    //        {
    //            kanbanData.ColumnTitles[columnIndex] = evt.newValue;
    //            DebounceAndSaveColumnTitles(() => {MarkDirtyAndSave();}, columnTitle);
    //        });
    //    }

    //    foreach (var columnTitle in kanbanData.ColumnTitles)
    //    {
    //        LoadSavedColumnData(columnTitle);
    //    }
    //}

    private void LoadSavedColumnData(ColumnData columnData)
    {
        VisualElement columnContainer = rootVisualElement.Q<VisualElement>("ColumnContainer");

        var taskColumnTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskColumn.uxml");
        var taskColumnElement = taskColumnTemplate.Instantiate();

        var titleField = taskColumnElement.Q<TextField>("ColumnTitle");
        titleField.value = columnData.columnTitle;
        titleField.RegisterValueChangedCallback(evt =>
        {
            columnData.columnTitle = evt.newValue;
            DebounceAndSaveColumnTitles(() => { MarkDirtyAndSave(); }, titleField);
        });

        var taskBox = taskColumnElement.Q<VisualElement>("TaskBox");
        foreach (var task in columnData.tasks)
        {
            var taskCard = InitTaskCards(taskBox, task);
            taskBox.Add(taskCard);
        }

        columnContainer.Add(taskColumnElement);
    }

    private void LoadSavedTaskData()
        // Checks for Tasks inside kanbanData.Tasks and delegates init to InitTaskCards
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

                InitTaskCards(taskCard, task);

                // Assign task card to its parent column
                if (task.parentColumnIndex >= 0 && task.parentColumnIndex < taskColumns.Count)
                {
                    var parentColumn = taskColumns[task.parentColumnIndex].Q<VisualElement>("TaskBox");
                    parentColumn.Add(taskCard);
                }
            }
        }
        else
        {
            Debug.Log("Cannot find TaskCard.uxml file. Check the path and file name.");
        }
    }

    private VisualElement InitTaskCards(VisualElement taskCard, TaskData task)
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
        taskText.RegisterValueChangedCallback(evt => DebounceAndSaveTaskCards(() => task.taskText = evt.newValue, taskCard, task));
        taskColour.RegisterValueChangedCallback(evt => DebounceAndSaveTaskCards(() => task.taskColour = evt.newValue, taskCard, task));
        stateDropdown.RegisterValueChangedCallback(evt => DebounceAndSaveTaskCards(() => task.taskState = (KanbanTaskState)evt.newValue, taskCard, task));

        // Register callbacks for drag and drop
        taskCard.RegisterCallback<PointerDownEvent>(evt => OnTaskPointerDown(evt, taskCard));
        taskCard.RegisterCallback<PointerMoveEvent>(evt => OnTaskPointerMove(evt, taskCard));
        taskCard.RegisterCallback<PointerUpEvent>(evt => OnTaskPointerRelease(evt, taskCard));

        return taskCard;
    }

    // Variables to handle dragging and dropping
    private VisualElement draggedTaskCard;
    private Vector2 dragOffset;

    private VisualElement originalParent;
    private Vector2 originalPosition;

    private void OnTaskPointerDown(PointerDownEvent evt, VisualElement taskCard)
    {
        draggedTaskCard = taskCard;
        dragOffset = evt.localPosition;

        originalParent = taskCard.parent; // to reset to original parent
        originalPosition = new Vector2(taskCard.resolvedStyle.left, taskCard.resolvedStyle.top); // to reset to original pos

        taskCard.CaptureMouse(); // Capture mouse events for the task card
    }

    private void OnTaskPointerMove(PointerMoveEvent evt, VisualElement taskCard)
    {
        if (draggedTaskCard != null && taskCard.HasMouseCapture()) // checking if taskcard is captured by mouse on TaskPointerDown
        {
            Vector2 newCardGlobalPosition = (Vector2)evt.position - dragOffset;

            // converts the global position to the local position within the parent container 
            // takes the mouse pos relative to the entire window (above) and converts to local Element Pos
            Vector2 newCardLocalPosition = taskCard.parent.WorldToLocal(newCardGlobalPosition);

            rootVisualElement.Add(taskCard); // making the root element while dragging fixes card positioning issues

            // Update card's position (the -15 corrects the position of the card relative to the mouse a bit)
            taskCard.style.left = newCardLocalPosition.x -15;
            taskCard.style.top = newCardLocalPosition.y;
        }
    }

    private void OnTaskPointerRelease(PointerUpEvent evt, VisualElement taskCard)
    {
        if (draggedTaskCard != null && taskCard.HasMouseCapture())
        {
            taskCard.ReleaseMouse();

            VisualElement newParent = null;
            int newParentIndex = -1;

            // Find the column the task card was dropped into
            for (int i = 0; i < taskColumns.Count; i++)
            {
                var column = taskColumns[i];
                var taskContainer = column.Q<VisualElement>("TaskBox");
                if (taskContainer.worldBound.Contains(evt.position))
                {
                    newParent = taskContainer;
                    newParentIndex = i;
                    break;
                }
            }

            // Checking for BoardEditor to put TaskCard back (to delete etc.)
            var boardEditor = rootVisualElement.Q<VisualElement>("BoardEditor");
            var newTaskBox = rootVisualElement.Q<VisualElement>("NewTaskBox");
            if (boardEditor.worldBound.Contains(evt.position))
            {
                newParent = newTaskBox;
            }

            draggedTaskCard = null; // Reset the dragged task card

            if (newParent != null) // Tweaking the TaskCards pos in newParent
            {
                newParent.Add(taskCard);

                taskCard.style.left = 0;
                taskCard.style.top = 0;

                //int newStateIndex = taskColumns.IndexOf(newParent) + 1;
                //taskState = (KanbanTaskState)newStateIndex;

                MarkDirtyAndSave();
            }
            else // Reset to original parent if not dropped in a valid column
            {
                originalParent.Add(taskCard);
                taskCard.style.left = originalPosition.x;
                taskCard.style.top = originalPosition.y;
            }
        }
    }

    //private void ApplyVisualOnState()
    //{
    //    apply visual changes based on the task state

    //    switch (newstate)
    //    {
    //        case kanbantaskstate.working:
    //            taskcard.style.backgroundcolor = new color(0.5f, 0.5f, 1f); // blue
    //            break;
    //        case kanbantaskstate.bugged:
    //            taskcard.style.backgroundcolor = new color(1f, 0.5f, 0.5f); // red
    //            break;
    //    }
    //}

    private void DebounceAndSaveColumnTitles(Action updateAction, TextField columnTitle)
    {
        updateAction.Invoke();
        DebounceUtility.Debounce(() =>
        {
            MarkDirtyAndSave();
        }, .5f);
    }

    private void DebounceAndSaveTaskCards(Action updateAction, VisualElement taskcard, TaskData task)
    {
        updateAction.Invoke();
        DebounceUtility.Debounce(() =>
        {
            MarkDirtyAndSave();
        }, .5f);
    }
}