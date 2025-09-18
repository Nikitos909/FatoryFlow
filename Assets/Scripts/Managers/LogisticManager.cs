using System.Collections.Generic;
using UnityEngine;

public class LogisticManager : MonoBehaviour
{
    public static LogisticManager Instance;

    public List<Logist> AllLogists = new List<Logist>();
    public Queue<TransportTask> PendingTasks = new Queue<TransportTask>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterLogist(Logist logist)
    {
        AllLogists.Add(logist);
    }

    public void AddTransportTask(Product product, Machine sourceMachine)
    {
        TransportTask newTask = new TransportTask
        {
            Product = product,
            SourceMachine = sourceMachine,
            Status = TaskStatus.Pending
        };
        
        PendingTasks.Enqueue(newTask);
        AssignTasks();
    }

    public void UpdateLogistics(float deltaTime)
    {
        foreach (Logist logist in AllLogists)
        {
            if (logist.IsAvailable)
            {
                AssignTasks();
            }
            else
            {
                logist.UpdateMovement(deltaTime);
            }
        }
    }

    private void AssignTasks()
    {
        foreach (Logist logist in AllLogists)
        {
            if (logist.IsAvailable && PendingTasks.Count > 0)
            {
                TransportTask task = PendingTasks.Dequeue();
                logist.AssignTask(task);
                task.Status = TaskStatus.InProgress;
            }
        }
    }
}
