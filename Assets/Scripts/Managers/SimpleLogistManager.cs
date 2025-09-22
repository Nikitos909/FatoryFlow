using UnityEngine;
using System.Collections.Generic;

public class SimpleLogisticsManager : MonoBehaviour
{
    public static SimpleLogisticsManager Instance;
    
    public List<SimpleLogist> availableLogists = new List<SimpleLogist>();
    private Queue<Machine> pendingTasks = new Queue<Machine>();

    void Awake()
    {
        Instance = this;
    }

    public void OnProductCreated(Machine machine)
    {
        pendingTasks.Enqueue(machine);
        TryAssignTask();
    }

    public void OnTaskCompleted(SimpleLogist logist)
    {
        availableLogists.Add(logist);
        TryAssignTask();
    }

    private void TryAssignTask()
    {
        if (pendingTasks.Count > 0 && availableLogists.Count > 0)
        {
            Machine sourceMachine = pendingTasks.Dequeue();
            Machine destinationMachine = FindDestinationMachine(sourceMachine.machineType.outputProductType);
            
            if (destinationMachine != null)
            {
                SimpleLogist logist = availableLogists[0];
                availableLogists.RemoveAt(0);
                
                logist.AssignTask(sourceMachine, destinationMachine);
            }
        }
    }

    private Machine FindDestinationMachine(ProductType productType)
    {
        foreach (Machine machine in FindObjectsOfType<Machine>())
        {
            if (machine.CanAcceptInput(productType))
                return machine;
        }
        return null;
    }
}
