using System.Collections.Generic;
using UnityEngine;

public class LogisticsManager1 : MonoBehaviour
{
    public static LogisticsManager1 Instance;

    [SerializeField] private List<Logist1> availableLogists = new List<Logist1>();

    private Queue<TransportTask1> pendingTasks = new Queue<TransportTask1>();
    private List<TransportTask1> assignedTasks = new List<TransportTask1>();

    public void Initialize()
    {
        if (Instance == null) Instance = this;
    }

    public void AddTask(TransportTask1 task)
    {
        pendingTasks.Enqueue(task);
        TryAssignTasks();
    }

    public void OnLogistBecameAvailable(Logist1 logist)
    {
        if (!availableLogists.Contains(logist))
        {
            availableLogists.Add(logist);
        }
        TryAssignTasks();
    }

    private void TryAssignTasks()
    {
        while (pendingTasks.Count > 0 && availableLogists.Count > 0)
        {
            TransportTask1 task = pendingTasks.Dequeue();
            Logist1 logist = availableLogists[0];
            availableLogists.RemoveAt(0);

            assignedTasks.Add(task);
            logist.AssignTask(task);
        }
    }

    public void OnTaskCompleted(TransportTask1 task, Logist1 logist)
    {
        assignedTasks.Remove(task);
        availableLogists.Add(logist);
        TryAssignTasks();
    }
}
