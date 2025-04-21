using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KanbanBoardDataManager", menuName = "My Tools/Kanban Board Data Manager")]
public class KanbanBoardDataManager : ScriptableObject
{
    public List<ColumnData> Columns = new List<ColumnData>();
    public List<TaskData> UnassignedTaskBox = new List<TaskData>();
    public int sliderValue;
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
    public List<TaskData> tasks = new List<TaskData>();
}

public enum KanbanTaskState
{
    Working,
    Bugged,
    LowPriority,
    MediumPriority,
    HighPriority,
}