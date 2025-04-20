using System;
using System.Linq;
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
        }

        int initialColumnCount = 4; // Initial number of columns

        while (kanbanData.Columns.Count < initialColumnCount)
        {
            int columnIndex = kanbanData.Columns.Count;
            kanbanData.Columns.Add(new ColumnData { columnTitle = $"Edit Column Title: {columnIndex + 1}" });
        }
        // kanbanData.sliderValue = 0; // Set the initial slider value
        MarkDirtyAndSave();

        GenerateWindowUI();

        foreach (var columnData in kanbanData.Columns)
        {
            LoadSavedColumnData(columnData);

            var columnContainer = rootVisualElement.Q<VisualElement>("ColumnContainer");
            var columnIndex = kanbanData.Columns.IndexOf(columnData);
            var taskContainer = columnContainer.Children().ElementAt(columnIndex).Q<VisualElement>("TaskBox");

            foreach (var task in columnData.tasks)
            {
                LoadSavedTaskData(task, taskContainer);
            }
        }

        var extraColumnSlider = rootVisualElement.Q<SliderInt>("ExtraColumnSlider");
        if (extraColumnSlider != null)
        {
            extraColumnSlider.value = kanbanData.sliderValue;
        }
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
        //int initialColumnCount = 4; // Initial number of columns

        //while (kanbanData.Columns.Count < initialColumnCount)
        //{
        //    kanbanData.Columns.Add(new ColumnData { columnTitle = $"Edit Column Title: {initialColumnCount + 1}" });
        //}
        //MarkDirtyAndSave();

        //// Remove excess columns if they exist
        //while (kanbanData.Columns.Count > initialColumnCount)
        //{
        //    kanbanData.Columns.RemoveAt(kanbanData.Columns.Count - 1);
        //}

        foreach (var columnData in kanbanData.Columns)
        {
            // Load the column data into the UI
            LoadSavedColumnData(columnData);
        }

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

            kanbanData.sliderValue = sliderValue; // Save the slider value to the ScriptableObject

            while (kanbanData.Columns.Count < totalColumns)
            {
                int columnIndex = kanbanData.Columns.Count;
                var newColumn = new ColumnData { columnTitle = $"Edit Column Title: {columnIndex + 1}" }; // Persist new column title
                kanbanData.Columns.Add(newColumn);

                LoadSavedColumnData(newColumn);
            }

            while (kanbanData.Columns.Count > totalColumns)
            {
                if (kanbanData.Columns.Count <= 4) break; // Prevent removing the initial columns

                kanbanData.Columns.RemoveAt(kanbanData.Columns.Count - 1);

                var lastColumn = rootVisualElement.Q<VisualElement>("ColumnContainer").Children().Last();
                columnContainer.Remove(lastColumn);

            }

            MarkDirtyAndSave();
        });

        // Button for adding new task into the BoardEditor
        addTaskButton.RegisterCallback<ClickEvent>(evt =>
        {
            //VisualElement boardEditorSlot = rootVisualElement.Q<VisualElement>("NewTaskBox");

            int targetColumnIndex = 0;
            var targetColumn = kanbanData.Columns[targetColumnIndex];

            TaskData newTask = new TaskData
            {
                taskText = "New Task",
                taskColour = Color.white,
                taskState = KanbanTaskState.Bugged,
                parentColumnIndex = targetColumnIndex
            };

            targetColumn.tasks.Add(newTask);

            var taskCardTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskCard.uxml");
            VisualElement taskCard = taskCardTemplate.Instantiate();

            //LoadSavedTaskData(newTask, taskCard);

            var taskBox = rootVisualElement.Q<VisualElement>("NewTaskBox");
            taskBox.Add(taskCard);

            LoadSavedTaskData(newTask, taskCard);

            MarkDirtyAndSave();


            //// Add a new task to the new task box (board editor)
            //TaskData newTask = new TaskData();
            //ColumnData.Tasks.Add(newTask);

            //// Instantiating a card for the new task
            //VisualElement taskCard = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskCard.uxml").Instantiate();

            //if (taskCard == null)
            //{
            //    Debug.Log("Instantiating TaskCard failed");
            //    return;
            //}

            //// THIS IS GENERATING A FRESH TASK CARD FOR THE NEW TASK
            //InitTaskCards(taskCard, newTask);
            //boardEditorSlot.Add(taskCard);

            //MarkDirtyAndSave();
        });

        // Button for deleting the last task in the BoardEditor
        // *Maybe try and make it so that the user can delete a selected task in the future*
        deleteTaskButton.RegisterCallback<ClickEvent>(evt =>
        {
            int targetColumnIndex = 0;
            var targetColumn = kanbanData.Columns[targetColumnIndex];

            if (targetColumn.tasks.Count > 0)
            {
                targetColumn.tasks.RemoveAt(targetColumn.tasks.Count - 1);

                var taskBox = rootVisualElement.Q<VisualElement>("NewTaskBox");
                taskBox.RemoveAt(taskBox.childCount - 1);

                MarkDirtyAndSave();
            }
            else
            {
                Debug.LogWarning("No tasks to delete in the the Board Editor");
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

        foreach (var child in columnContainer.Children())
        {
            var existingTitleField = child.Q<TextField>("ColumnTitle");
            if (existingTitleField != null && existingTitleField.value == columnData.columnTitle)
            {
                Debug.LogWarning($"Column with title '{columnData.columnTitle}' already exists in the UI.");
                return; // Prevent duplicate instantiation
            }
        }

        var taskColumnTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskColumn.uxml");
        var taskColumnElement = taskColumnTemplate.Instantiate();

        var titleField = taskColumnElement.Q<TextField>("ColumnTitle");
        titleField.value = columnData.columnTitle;
        titleField.RegisterValueChangedCallback(evt => DebounceAndSaveColumnTitles(() => columnData.columnTitle = evt.newValue, titleField));

        var taskBox = taskColumnElement.Q<VisualElement>("TaskBox");
        foreach (var task in columnData.tasks)
        {
            var taskCard = LoadSavedTaskData(task, taskBox);
            taskBox.Add(taskCard);
        }

        columnContainer.Add(taskColumnElement);
    }

    //private void LoadSavedTaskData()
    //    // Checks for Tasks inside kanbanData.Tasks and delegates init to InitTaskCards
    //{
    //    var taskCardTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskCard.uxml");
    //    if (taskCardTemplate != null)
    //    {
    //        // Generating Task Cards
    //        foreach (var task in kanbanData.Tasks)
    //        {
    //            // Instantiating a card for each task that exists
    //            VisualElement taskCard = taskCardTemplate.Instantiate();

    //            if (taskCard == null)
    //            {
    //                Debug.Log("Instantiating TaskCard failed");
    //                continue;
    //            }

    //            InitTaskCards(taskCard, task);

    //            // Assign task card to its parent column
    //            if (task.parentColumnIndex >= 0 && task.parentColumnIndex < taskColumns.Count)
    //            {
    //                var parentColumn = taskColumns[task.parentColumnIndex].Q<VisualElement>("TaskBox");
    //                parentColumn.Add(taskCard);
    //            }
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("Cannot find TaskCard.uxml file. Check the path and file name.");
    //    }
    //}

    private VisualElement LoadSavedTaskData(TaskData taskData, VisualElement taskContainer)
    {
        // Loading individual cards into UXML depending on how many tasks exist
        var taskCardTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskCard.uxml");
        var taskCard = taskCardTemplate.Instantiate();

        taskCard.userData = taskData;

        TextField taskText = taskCard.Q<TextField>("TaskText");
        EnumField stateDropdown = taskCard.Q<EnumField>("TaskState");
        ColorField taskColour = taskCard.Q<ColorField>("TaskColor");

        // Initializing the TaskText Colour and State
        taskText.value = taskData.taskText;
        taskColour.value = taskData.taskColour;
        stateDropdown.value = taskData.taskState;
        //stateDropdown.Init(/*initialise in the board editor*/);

        // Register callbacks for updating the task data (task, state, colour)
        taskText.RegisterValueChangedCallback(evt => DebounceAndSaveTaskCards(() => taskData.taskText = evt.newValue, taskCard, taskData));
        taskColour.RegisterValueChangedCallback(evt => DebounceAndSaveTaskCards(() => taskData.taskColour = evt.newValue, taskCard, taskData));
        stateDropdown.RegisterValueChangedCallback(evt => DebounceAndSaveTaskCards(() => taskData.taskState = (KanbanTaskState)evt.newValue, taskCard, taskData));

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

            VisualElement newParentTaskBox = null;
            int newParentIndex = -1;

            var columnContainer = rootVisualElement.Q<VisualElement>("ColumnContainer");
            foreach (var column in columnContainer.Children())
            {
                if (column.worldBound.Contains(evt.position))
                {
                    newParentTaskBox = column.Q<VisualElement>("TaskBox");
                    newParentIndex = columnContainer.IndexOf(column);
                    break;
                }
            }

            if (newParentTaskBox != null && newParentIndex != -1)
            {
                newParentTaskBox.Add(taskCard); // Add the task card to the new parent task box

                foreach (var columnData in kanbanData.Columns)
                {
                    if (columnData.tasks.Remove(taskCard.userData as TaskData)) break;
                }

                var taskData = taskCard.userData as TaskData;
                if (taskData != null)
                {
                    kanbanData.Columns[newParentIndex].tasks.Add(taskData); // Add the task data to the new parent column
                    taskData.parentColumnIndex = newParentIndex; // Update the parent column index
                    MarkDirtyAndSave();
                }

            }


            #region Resetting TaskCard if dragged Out of Bounds

            // Checking for BoardEditor to put TaskCard back (to delete etc.)
            var boardEditor = rootVisualElement.Q<VisualElement>("BoardEditor");
            var newTaskBox = rootVisualElement.Q<VisualElement>("NewTaskBox");
            if (boardEditor.worldBound.Contains(evt.position))
            {
                newParentTaskBox = newTaskBox;
            }

            draggedTaskCard = null; // Reset the dragged task card

            if (newParentTaskBox != null) // Tweaking the TaskCards pos in newParent
            {
                newParentTaskBox.Add(taskCard);

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

            #endregion
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