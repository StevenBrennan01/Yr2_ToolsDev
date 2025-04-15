using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KanbanBoardDataManager", menuName = "My Tools/Kanban Board Data Manager")]
public class KanbanBoardDataManager : ScriptableObject
{
    // List to manage Tasks
    public List<KanbanTask> Tasks = new List<KanbanTask>();

    // List to manage Column Titles
    public List<string> ColumnTitles;
}

[System.Serializable]
public class KanbanTask
{
    public string taskText;
    public KanbanTaskState taskState;
    public Color taskColour;
    public int parentColumnIndex;
}

public enum KanbanTaskState
{
    Working,
    Bugged,
    LowPriority,
    MediumPriority,
    HighPriority,
}