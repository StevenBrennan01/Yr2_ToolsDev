using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KanbanBoardDataManager", menuName = "My Tools/Kanban Board Data Manager")]
public class KanbanBoardDataManager : ScriptableObject
{
    public List<ColumnData> columns = new List<ColumnData>();
    public List<TaskData> unassignedTaskBox = new List<TaskData>();
    public List<string> backgroundList = new List<string> { "None", "Cityscape", "Waterfall", "Ravine" };
    public string selectedBackground = "None";

    public int sliderValue;
    public string dueDate = "DD/MM/YYYY";
    public string projectTitle = "Edit Project Title:";
    public void ResetKanbanData()
    {
        columns.Clear();
        unassignedTaskBox.Clear();
        sliderValue = 0;
        dueDate = "DD/MM/YYYY";
        projectTitle = "Edit Project Title:";
        selectedBackground = "None";
    }
}

[System.Serializable]
public class TaskData
{
    public string taskText;
    public Color taskColour;
    public KanbanTaskState taskState;
    public int parentColumnIndex;
}

[System.Serializable]
public class ColumnData
{
    public string columnTitle;
    public Color titleBorderColor = new Color(255f/255f, 255f/255f, 255f/255f, 1);
    public List<TaskData> tasks = new List<TaskData>();
}

public enum KanbanTaskState
{
    Unassigned,
    Working,
    Bugged,
    NeedsPolish,
    Completed,
    LowPriority,
    MediumPriority,
    HighPriority,
}