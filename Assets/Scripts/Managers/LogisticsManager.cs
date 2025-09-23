using UnityEngine;
using System.Collections.Generic;

public class LogisticsManager : MonoBehaviour
{
    public static LogisticsManager Instance;

    public List<Logist> availableLogists = new List<Logist>();
    private Queue<Machine> machinesWithProducts = new Queue<Machine>();

    void Awake()
    {
        Instance = this;
    }

    public void OnProductProduced(Machine machine)
    {
        machinesWithProducts.Enqueue(machine);
        TryAssignTasks();
    }

    public void OnTaskCompleted(Logist logist)
    {
        availableLogists.Add(logist);
        TryAssignTasks();
    }

    private void TryAssignTasks()
    {
        while (machinesWithProducts.Count > 0 && availableLogists.Count > 0)
        {
            Machine sourceMachine = machinesWithProducts.Dequeue();
            Machine destinationMachine = FindDestinationMachine(sourceMachine.machineType.outputProductType);

            if (destinationMachine != null && destinationMachine.CanAcceptInput(sourceMachine.machineType.outputProductType))
            {
                Logist logist = availableLogists[0];
                availableLogists.RemoveAt(0);

                logist.AssignTask(sourceMachine, destinationMachine);
                Debug.Log("Назначена задача логисту");
            }
        }
    }

    private Machine FindDestinationMachine(ProductType productType)
    {
        foreach (Machine machine in FindObjectsOfType<Machine>())
        {
            if (machine.machineType.inputProductType == productType && machine.CanAcceptInput(productType))
                return machine;
        }
        return null;
    }
}