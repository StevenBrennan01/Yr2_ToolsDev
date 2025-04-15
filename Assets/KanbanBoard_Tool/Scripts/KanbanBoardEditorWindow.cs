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

        InitBaseUiElements();
    }

    private void InitBaseUiElements()
    {
        taskColumns = new List<VisualElement>();

        for (int i = 0; i < 4; i++)
        {
            InitColumns($"Edit Column Title:{i + 1}");
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

        LoadSavedColumnData();
        LoadSavedTaskData();
    }

    private void LoadSavedColumnData()
    {
        for (int i = 0; i < kanbanData.ColumnTitles.Count; i++)
        {
            if (i >= taskColumns.Count)
            {
                Debug.Log("Column not found, check the number of columns in the kanbanData");
                continue;
            }

            VisualElement taskColumn = taskColumns[i];
            TextField columnTitle = taskColumn.Q<TextField>("ColumnTitle");

            columnTitle.value = kanbanData.ColumnTitles[i];

            int columnIndex = i; // Capture the current index for the callback
            columnTitle.RegisterValueChangedCallback(evt =>
            {
                kanbanData.ColumnTitles[columnIndex] = evt.newValue;
                DebounceAndSaveColumnTitles(() => {MarkDirtyAndSave();}, columnTitle);
            });
        }
    }

    private void InitColumns(string columnName)
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
                //MarkDirtyAndSave();
            }
        }
        else
        {
            Debug.Log("Cannot find TaskCard.uxml file. Check the path and file name.");
        }
    }

    private VisualElement InitTaskCards(VisualElement taskCard, KanbanTask task)
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
        stateDropdown.RegisterValueChangedCallback(evt =>
        {
            DebounceAndSaveTaskCards(() => task.taskState = (KanbanTaskState)evt.newValue, taskCard, task);
        });

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

    private Coroutine resetTaskCard;

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
            // Checking for task columns to drop the task card
            foreach (var column in taskColumns)
            {
                var taskContainer = column.Q<VisualElement>("TaskBox");
                if (taskContainer.worldBound.Contains(evt.position))
                {
                    newParent = taskContainer;
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

            if (newParent != null)
            {
                newParent.Add(taskCard);

                taskCard.style.left = 0;
                taskCard.style.top = 0;

                //int newStateIndex = taskColumns.IndexOf(newParent) + 1;
                //taskState = (KanbanTaskState)newStateIndex;

                MarkDirtyAndSave();
            }
            else
            {
                originalParent.Add(taskCard); // Reset to original parent if not dropped in a valid column
                taskCard.style.left = originalPosition.x;
                taskCard.style.top = originalPosition.y;

                //AnimateBackToOriginalPosition(taskCard, originalParent, originalPosition);
            }

            draggedTaskCard = null; // Reset the dragged task card
        }
    }

    //private void AnimateBackToOriginalPosition(VisualElement taskCard, VisualElement originalParent, Vector2 originalPosition)
    //{
    //    // Step 1: Capture the global position before moving the task card
    //    Rect globalBounds = taskCard.worldBound;
    //    Vector2 globalStartPosition = new Vector2(globalBounds.xMin, globalBounds.yMin);

    //    // Step 2: Re-add the task card to its original parent
    //    originalParent.Add(taskCard);

    //    // Step 3: Convert the global start position to the local position within the original parent
    //    Vector2 localStartPosition = originalParent.WorldToLocal(globalStartPosition);

    //    // Step 4: Start the animation
    //    float duration = 0.3f; // Duration of the animation in seconds
    //    float elapsedTime = 0f;

    //    void UpdateAnimation()
    //    {
    //        elapsedTime += Time.deltaTime;
    //        float t = Mathf.Clamp01(elapsedTime / duration); // Normalize time

    //        // Smoothly interpolate the position
    //        Vector2 newPosition = Vector2.Lerp(localStartPosition, originalPosition, t);
    //        taskCard.style.left = newPosition.x;
    //        taskCard.style.top = newPosition.y;

    //        if (t >= 1f)
    //        {
    //            // Stop the animation when done
    //            EditorApplication.update -= UpdateAnimation;

    //            // Ensure exact final position
    //            taskCard.style.left = originalPosition.x;
    //            taskCard.style.top = originalPosition.y;
    //        }
    //    }

    //    // Register the update callback
    //    EditorApplication.update += UpdateAnimation;
    //}

    private void ApplyVisualOnState()
    {
        // Apply visual changes based on the task state

        //switch (newState)
        //{
        //    case KanbanTaskState.Working:
        //        taskCard.style.backgroundColor = new Color(0.5f, 0.5f, 1f); // Blue
        //        break;
        //    case KanbanTaskState.Bugged:
        //        taskCard.style.backgroundColor = new Color(1f, 0.5f, 0.5f); // Red
        //        break;
        //}
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