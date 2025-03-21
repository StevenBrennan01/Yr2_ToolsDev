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
    public string taskText;
    public KanbanTaskState taskState;
    public Color taskColour;
}

public enum KanbanTaskState
{
    NewTask,
    ToDo,
    InProgress,
    ToPolish,
    Finished
}