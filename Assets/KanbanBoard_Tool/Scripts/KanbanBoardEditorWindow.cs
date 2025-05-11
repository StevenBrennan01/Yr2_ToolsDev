using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class KanbanBoardEditorWindow : EditorWindow
{
    private KanbanBoardDataManager kanbanData;

    [MenuItem("Kanban Tool/Custom Kanban Board")]
    public static void OpenWindow()
    {
        var window = GetWindow<KanbanBoardEditorWindow>("Kanban Board");
        window.minSize = new Vector2(1000, 500);
    }

    private void OnEnable()
    {
        string dataAssetPath = "Assets/KanbanBoard_Tool/KanbanDataSO/KanbanBoardDataManager.asset";

        kanbanData = AssetDatabase.LoadAssetAtPath<KanbanBoardDataManager>(dataAssetPath);

        if (kanbanData == null)
        {
            kanbanData = CreateInstance<KanbanBoardDataManager>();
            AssetDatabase.CreateAsset(kanbanData, dataAssetPath);
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

        InitUIElements();
    }

    private void InitUIElements()
    {
        // Add/Delete task buttons
        Button addTaskButton = rootVisualElement.Q<Button>("AddTaskButton");
        Button deleteTaskButton = rootVisualElement.Q<Button>("DeleteTaskButton");
        Button resetDataButton = rootVisualElement.Q<Button>("ResetDataButton");

        VisualElement columnContainer = rootVisualElement.Q<VisualElement>("ColumnContainer");
        VisualElement boardEditorBox = rootVisualElement.Q<VisualElement>("BoardEditorBox");
        VisualElement kanbanContainer = rootVisualElement.Q<VisualElement>("KanbanContainer");

        SliderInt extraColumnSlider = rootVisualElement.Q<SliderInt>("ExtraColumnSlider");

        TextField dueDate = rootVisualElement.Q<TextField>("DueDateField");
        TextField projectTitle = rootVisualElement.Q<TextField>("ProjectTitleField");

        DropdownField backgroundOptions = rootVisualElement.Q<DropdownField>("BackgroundDropdown");
        var initBackground = EditorGUIUtility.Load($"Assets/KanbanBoard_Tool/Window_UI/Backgrounds/{kanbanData.selectedBackground}.png") as Texture2D;
        kanbanContainer.style.backgroundImage = new StyleBackground(initBackground);

        // Background Options
        backgroundOptions.choices = kanbanData.backgroundList;
        backgroundOptions.value = kanbanData.selectedBackground; // Set the initial value to the first background option
        backgroundOptions.RegisterValueChangedCallback(evt =>
        {
            string selectedBackground = evt.newValue;
            var bgTexture = EditorGUIUtility.Load($"Assets/KanbanBoard_Tool/Window_UI/Backgrounds/{selectedBackground}.png") as Texture2D;
            kanbanContainer.style.backgroundImage = new StyleBackground(bgTexture);

            kanbanData.selectedBackground = selectedBackground; // Save the selected background to the ScriptableObject

            MarkDirtyAndSave();
        });

        // Project Title Setup
        projectTitle.value = kanbanData.projectTitle;
        projectTitle.RegisterValueChangedCallback(evt =>
        {
            kanbanData.projectTitle = evt.newValue;

            if(projectTitle.value.Length == 0)
            {
                kanbanData.projectTitle = "Edit Project Title:";

                DebounceUtility.Debounce(() =>
                {
                    MarkDirtyAndSave();
                }, .5f);
            }
            else
            {
                DebounceUtility.Debounce(() =>
                {
                    MarkDirtyAndSave();
                }, .5f);
            }
        });

        // Due Date Setup
        dueDate.value = kanbanData.dueDate;
        dueDate.RegisterValueChangedCallback(evt =>
        {
            string dateInput = evt.newValue;

            // Creates a new string that removes any non integer characters
            dateInput = new string(dateInput.Where(c => char.IsDigit(c)).ToArray());

            // Formatting the input as DD/MM/YYYY
            if (dateInput.Length > 2)
                dateInput = dateInput.Insert(2, "/");
            if (dateInput.Length > 5)
                dateInput = dateInput.Insert(5, "/");
            if (dateInput.Length > 10)
                dateInput = dateInput.Substring(0, 10); // Limit to 10 characters

            dueDate.SetValueWithoutNotify(dateInput); // Update the field without triggering the callback

            int caretPosition = dueDate.cursorIndex;

            // Set/Bump forward the caret position because it isn't
            // updating programmatically as the date is being formatted
            if (caretPosition == 2 || caretPosition == 5)
            {
                caretPosition++;
            }

            dueDate.cursorIndex = caretPosition;

            if (dateInput.Length == 10)
            {
                kanbanData.dueDate = dateInput;
                DebounceUtility.Debounce(() =>
                {
                    MarkDirtyAndSave();
                }, .5f);
            }
            else if (dateInput.Length == 0)
            {
                kanbanData.dueDate = "DD/MM/YYYY";
                DebounceUtility.Debounce(() =>
                {
                    MarkDirtyAndSave();
                }, .5f);
            }
        });

        // Column Setup
        int initialColumnCount = 4; // Initial number of columns

        while (kanbanData.columns.Count < initialColumnCount)
        {
            int columnIndex = kanbanData.columns.Count;
            kanbanData.columns.Add(new ColumnData { columnTitle = $"Edit Column Title: {columnIndex + 1}" });
        }

        foreach (var task in kanbanData.unassignedTaskBox)
        {
            var taskCard = LoadSavedTaskData(task, boardEditorBox);
            boardEditorBox.Add(taskCard);
        }

        foreach (var columnData in kanbanData.columns)
        {
            LoadSavedColumnData(columnData);

            var columnIndex = kanbanData.columns.IndexOf(columnData);
            var taskContainer = columnContainer.Children().ElementAt(columnIndex).Q<VisualElement>("TaskBox");

            foreach (var task in columnData.tasks)
            {
                LoadSavedTaskData(task, taskContainer);
            }
        }

        // Set the slider value to the current number of extra columns
        if (extraColumnSlider != null)
        {
            extraColumnSlider.value = kanbanData.sliderValue;
        }

        #region Extra Column Slider
        extraColumnSlider.RegisterValueChangedCallback(evt =>
        {
            int sliderValue = (int)evt.newValue;
            int totalColumns = initialColumnCount + sliderValue;

            kanbanData.sliderValue = sliderValue; // Save slider value to ScriptableObject

            while (kanbanData.columns.Count < totalColumns)
            {
                int columnIndex = kanbanData.columns.Count;
                var newColumn = new ColumnData { columnTitle = $"Edit Column Title: {columnIndex + 1}" }; // Persist new column title
                kanbanData.columns.Add(newColumn);

                LoadSavedColumnData(newColumn);
            }

            while (kanbanData.columns.Count > totalColumns)
            {
                if (kanbanData.columns.Count <= initialColumnCount) break; // Prevent removing the initial columns

                kanbanData.columns.RemoveAt(kanbanData.columns.Count - 1);

                VisualElement lastColumn = rootVisualElement.Q<VisualElement>("ColumnContainer").Children().Last();
                columnContainer.Remove(lastColumn);

            }

            MarkDirtyAndSave();
        });
        #endregion

        #region Add/Delete Task Buttons + Reset Board Button

        // Button for adding new task into the BoardEditor
        addTaskButton.RegisterCallback<ClickEvent>(evt =>
        {
            if (kanbanData.unassignedTaskBox.Count > 0) // Only allowing one at a time
            {
                DisplayDebugMessage("There is already a task below. Please move it before adding a new one.", 5f);
                return;
            }

            TaskData newTask = new TaskData
            {
                taskText = "Edit Task Description:",
                taskColour = new Color(0f/255f, 182f/255f, 255f/255f, 1),
                taskState = KanbanTaskState.Unassigned,
                parentColumnIndex = -1 // -1 means not within a standard column
            };

            kanbanData.unassignedTaskBox.Add(newTask);

            var taskCard = LoadSavedTaskData(newTask, boardEditorBox);
            boardEditorBox.Add(taskCard);

            MarkDirtyAndSave();
        });

        // Button for deleting the last task in the BoardEditor
        // *Maybe try and make it so that the user can delete a selected task in the future*
        deleteTaskButton.RegisterCallback<ClickEvent>(evt =>
        {
            if (kanbanData.unassignedTaskBox.Count > 0)
            {
                kanbanData.unassignedTaskBox.Clear();
                boardEditorBox.Clear();
                MarkDirtyAndSave();
            }
        });

        //Button for RESETTING ALL DATA
        resetDataButton.RegisterCallback<ClickEvent>(evt =>
        {
            kanbanData.ResetKanbanData();

            while (kanbanData.columns.Count < initialColumnCount)
            {
                int columnIndex = kanbanData.columns.Count;
                kanbanData.columns.Add(new ColumnData { columnTitle = $"Edit Column Title: {columnIndex + 1}" });
            }

            columnContainer.Clear();
            boardEditorBox.Clear();
            extraColumnSlider.value = 0;
            dueDate.SetValueWithoutNotify("DD/MM/YYYY");
            projectTitle.SetValueWithoutNotify("Edit Project Title:");
            //kanbanData.selectedBackground = "None";

            foreach (var columnData in kanbanData.columns)
            {
                LoadSavedColumnData(columnData);

                var columnIndex = kanbanData.columns.IndexOf(columnData);
                var taskContainer = columnContainer.Children().ElementAt(columnIndex).Q<VisualElement>("TaskBox");
            }

            MarkDirtyAndSave();
        });
        #endregion
    }
    private double debugMessageHideTime = 0;
    private bool debugMessageVisible = false;
    private void DisplayDebugMessage(string message, float duration = 5f)
    {
        var debugBox = rootVisualElement.Q<Label>("DebugText");
        debugBox.text = message;
        debugBox.style.display = DisplayStyle.Flex;

        debugMessageHideTime = EditorApplication.timeSinceStartup + duration;
        if (!debugMessageVisible)
        {
            EditorApplication.update += DebugBoxUpdate;
            debugMessageVisible = true;
        }
    }

    private void DebugBoxUpdate() // Used to get the time since startup and hide the message after a duration
    {
        if (EditorApplication.timeSinceStartup >= debugMessageHideTime)
        {
            var debugBox = rootVisualElement.Q<Label>("DebugText");
            debugBox.style.display = DisplayStyle.None;
            EditorApplication.update -= DebugBoxUpdate;
            debugMessageVisible = false;
        }
    }

    private void LoadSavedColumnData(ColumnData columnData)
    {
        VisualElement columnContainer = rootVisualElement.Q<VisualElement>("ColumnContainer");

        foreach (var child in columnContainer.Children())
        {
            TextField existingTitleField = child.Q<TextField>("ColumnTitle");
            if (existingTitleField != null && existingTitleField.value == columnData.columnTitle)
            {
                DisplayDebugMessage($"Column with title '{columnData.columnTitle}' already exists in the UI.", 5f);
                return; // Prevent duplicate instantiation
            }
        }

        var taskColumnTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskColumn.uxml");
        var taskColumnElement = taskColumnTemplate.Instantiate();

        // Set the column title
        TextField titleField = taskColumnElement.Q<TextField>("ColumnTitle");
        titleField.value = columnData.columnTitle;
        titleField.RegisterValueChangedCallback(evt => DebounceAndSaveColumnTitles(() => columnData.columnTitle = evt.newValue, titleField));

        // Set the column title color
        ColorField columnTitleColour = taskColumnElement.Q<ColorField>("TitleColorEditor");
        columnTitleColour.value = columnData.titleBorderColor; // Set the initial color

        titleField.style.borderTopColor = columnData.titleBorderColor;
        titleField.style.borderBottomColor = columnData.titleBorderColor;
        titleField.style.borderLeftColor = columnData.titleBorderColor;
        titleField.style.borderRightColor = columnData.titleBorderColor;

        columnTitleColour.RegisterValueChangedCallback(evt =>
        {
            titleField.style.borderTopColor = evt.newValue;
            titleField.style.borderBottomColor = evt.newValue;
            titleField.style.borderLeftColor = evt.newValue;
            titleField.style.borderRightColor = evt.newValue;

            columnData.titleBorderColor = evt.newValue; // Save the new color to the ScriptableObject
            DebounceUtility.Debounce(() =>
            {
                MarkDirtyAndSave();
            }, .5f);
        });

        // Now checks for tasks that should be within these columns
        VisualElement taskBox = taskColumnElement.Q<VisualElement>("TaskBox");
        foreach (var task in columnData.tasks)
        {
            var taskCard = LoadSavedTaskData(task, taskBox);
            taskBox.Add(taskCard);
        }

        columnContainer.Add(taskColumnElement);
    }

    private VisualElement LoadSavedTaskData(TaskData taskData, VisualElement taskContainer)
    {
        // Loading individual cards into UXML depending on how many tasks exist
        var taskCardTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskCard.uxml");
        var taskCard = taskCardTemplate.Instantiate();

        taskCard.userData = taskData;

        TextField taskText = taskCard.Q<TextField>("TaskText");
        ColorField taskColour = taskCard.Q<ColorField>("TaskColor");
        EnumField stateDropdown = taskCard.Q<EnumField>("TaskState");
        stateDropdown.Init(KanbanTaskState.Unassigned); // Set the initial value of the dropdown
        stateDropdown.value = taskData.taskState; // Set the initial value of the dropdown to the task's state

        // Initializing the TaskText Colour and State
        taskText.value = taskData.taskText;
        taskColour.value = taskData.taskColour;
        stateDropdown.value = taskData.taskState;
       
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
        taskCard.CaptureMouse(); // Capture mouse events for the task card

        draggedTaskCard = taskCard;
        dragOffset = evt.localPosition;

        originalParent = taskCard.parent; // To reset to original parent
        originalPosition = new Vector2(taskCard.resolvedStyle.left, taskCard.resolvedStyle.top); // To reset to original pos
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
            taskCard.style.left = newCardLocalPosition.x - 5f;
            taskCard.style.top = newCardLocalPosition.y;
        }
    }

    private void OnTaskPointerRelease(PointerUpEvent evt, VisualElement taskCard)
    {
        if (draggedTaskCard != null && taskCard.HasMouseCapture())
        {
            taskCard.ReleaseMouse();

            VisualElement newParentTaskBox = null;
            int newParentColumnIndex = -1;

            var columnContainer = rootVisualElement.Q<VisualElement>("ColumnContainer");
            foreach (var column in columnContainer.Children())
            {
                if (column.worldBound.Contains(evt.position))
                {
                    VisualElement taskBox = column.Q<VisualElement>("TaskBox");

                    if (taskBox.childCount >= 10)
                    {
                        DisplayDebugMessage("This column is full, please drop in another column or the Board Editor.", 5f); // Display message if column is full
                        originalParent.Add(taskCard); // Reset to original parent if column is full
                        taskCard.style.left = originalPosition.x;
                        taskCard.style.top = originalPosition.y;
                        return;
                    }
                    else
                    {
                        newParentTaskBox = column.Q<VisualElement>("TaskBox");
                        newParentColumnIndex = columnContainer.IndexOf(column);
                        break;
                    }
                }
            }

            // Dropped into a column
            if (newParentTaskBox != null && newParentColumnIndex != -1)
            {
                var taskData = taskCard.userData as TaskData;

                // Remove from unassigned if present
                kanbanData.unassignedTaskBox.Remove(taskData);

                // Remove from all columns (in case it was moved from another column)
                foreach (var columnData in kanbanData.columns)
                {
                    columnData.tasks.Remove(taskData);
                }

                kanbanData.columns[newParentColumnIndex].tasks.Add(taskData);
                taskData.parentColumnIndex = newParentColumnIndex; // Update the parent column index

                newParentTaskBox.Add(taskCard); // Add the task card to the new parent task box

                // Reset card position so they sit in columns correctly

                ResetCardPosition(taskCard);

                MarkDirtyAndSave();
            }

            else // Checking if dropped in the BoardEditor
            {
                VisualElement boardEditor = rootVisualElement.Q<VisualElement>("BoardEditor"); // Allows the taskCard to be dropped in the BoardEditor
                VisualElement boardEditorBox = rootVisualElement.Q<VisualElement>("BoardEditorBox"); // Which then slots into the NewTaskBox

                if (boardEditor.worldBound.Contains(evt.position))
                {
                    newParentTaskBox = boardEditorBox;

                    var taskData = taskCard.userData as TaskData;

                    foreach (var columnData in kanbanData.columns)
                    {
                        columnData.tasks.Remove(taskData);
                    }

                    if (kanbanData.unassignedTaskBox.Count == 0 || kanbanData.unassignedTaskBox.Contains(taskData))
                    {
                        if (!kanbanData.unassignedTaskBox.Contains(taskData))
                            kanbanData.unassignedTaskBox.Add(taskData);

                        taskData.parentColumnIndex = -1; // Reset the parent column index

                        boardEditorBox.Add(taskCard); // Add the task card to the BoardEditor

                        ResetCardPosition(taskCard);

                        MarkDirtyAndSave();
                    }
                    else
                    {
                        // Already an unassigned task; snap back to original parent
                        originalParent.Add(taskCard);
                        taskCard.style.left = originalPosition.x;
                        taskCard.style.top = originalPosition.y;
                    }
                }
                else // Not dropped in a valid area; reset to original parent
                {
                    originalParent.Add(taskCard);
                    ResetCardPosition(taskCard); // Reset the position to original
                }
            }

            draggedTaskCard = null; // Reset the dragged task card
        }
    }

    //private void ApplyVisualOnState(VisualElement taskCard)
    //{
    //    //apply visual changes based on the task state

    //    switch ()
    //    {
    //        case KanbanTaskState.Working:
    //            taskCard.style.backgroundColor = new Color(0.5f, 0.5f, 1f); // blue
    //            break;
    //        case KanbanTaskState.Bugged:
    //            taskCard.style.backgroundColor = new Color(1f, 0.5f, 0.5f); // red
    //            break;
    //    }
    //}

    void ResetCardPosition(VisualElement taskCard)
    {
        taskCard.style.left = 0;
        taskCard.style.top = 0;
    }

    private void DebounceAndSaveColumnTitles(Action updateAction, TextField columnTitle)
    {
        updateAction.Invoke();
        DebounceUtility.Debounce(() =>
        {
            MarkDirtyAndSave();
        }, .5f);
    }

    private void DebounceAndSaveTaskCards(Action updateAction, VisualElement taskcard, TaskData taskData)
    {
        updateAction.Invoke();
        DebounceUtility.Debounce(() =>
        {
            MarkDirtyAndSave();
        }, .5f);
    }
}