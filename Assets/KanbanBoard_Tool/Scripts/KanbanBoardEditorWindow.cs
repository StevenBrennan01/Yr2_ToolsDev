using System;
using Unity.VisualScripting;
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

            if (kanbanData.Tasks.Count == 0)
            {
                Debug.Log("No tasks found in the data manager.");
            }

            // Generating Task Cards
            foreach (var task in kanbanData.Tasks)
            {
                VisualElement taskCard = CreateTaskCard(task);
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

    private VisualElement CreateTaskCard(KanbanTask task)
    {
        // Loading individual cards into UXML depending on how many tasks exist
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/KanbanBoard_Tool/Window_UI/TaskCard.uxml");
        VisualElement taskCard = visualTree.CloneTree();

        TextField taskText = taskCard.Q<TextField>("TaskText");
        EnumField stateDropdown = taskCard.Q<EnumField>("TaskState");

        taskText.value = task.taskTitle;
        stateDropdown.value = task.taskState;

        taskCard.RegisterCallback<PointerDownEvent>(evt => OnTaskPointerDown(evt, taskCard));
        taskCard.RegisterCallback<PointerDownEvent>(evt => OnTaskPointerDown(evt, taskCard));
        taskCard.RegisterCallback<PointerDownEvent>(evt => OnTaskPointerDown(evt, taskCard));

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