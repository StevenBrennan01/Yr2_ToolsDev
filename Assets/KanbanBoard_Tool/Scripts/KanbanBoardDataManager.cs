using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KanbanBoardDataManager", menuName = "My Tools/Kanban Board Data Manager")]
public class KanbanBoardDataManager : ScriptableObject
{
    public List<KanbanTask> Tasks = new List<KanbanTask>();
}

[System.Serializable]
public class KanbanTask
{
    public string taskTitle;
    public KanbanTaskState taskState;
}

public enum KanbanTaskState
{
    ToDo,
    InProgress,
    ToPolish,
    Finished
}