using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KanbanBoardDataManager", menuName = "My Tools/Kanban Board Data Manager")]
public class KanbanBoardDataManager : ScriptableObject
{
    public List<ColumnData> Columns = new List<ColumnData>();
    public List<TaskData> UnassignedTaskBox = new List<TaskData>();

    public int sliderValue;
    public string dueDate = "DD/MM/YYYY";
    public void ResetKanbanData()
    {
        Columns.Clear();
        UnassignedTaskBox.Clear();
        //for (int i = 0; i < 4; i++)
        //{
        //    Columns.Add(new ColumnData { columnTitle = $"Edit Column Title: {i + 1}", titleBorderColor = new Color(255f/255f, 95f/255f, 0f/225f, 1) });
        //}
        sliderValue = 0;
        dueDate = "DD/MM/YYYY";
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
    public Color titleBorderColor = new Color(255f/255f, 95f/255f, 0f/225f, 1);
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