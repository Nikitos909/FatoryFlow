using UnityEngine;
using System.Collections.Generic;

public class LogisticsManager : MonoBehaviour
{
    public static LogisticsManager Instance;

    public List<Logist> availableLogists = new List<Logist>();
    private Queue<Machine> pendingPickups = new Queue<Machine>();

    void Awake()
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

    public void OnProductProduced(Machine machine)
    {
        pendingPickups.Enqueue(machine);
        TryAssignTasks();
    }

    public void OnTaskCompleted(Logist logist)
    {
        if (!availableLogists.Contains(logist))
        {
            availableLogists.Add(logist);
        }
        TryAssignTasks();
    }

    private void TryAssignTasks()
    {
        while (pendingPickups.Count > 0 && availableLogists.Count > 0)
        {
            Machine sourceMachine = pendingPickups.Dequeue();

            if (sourceMachine.currentOutput == null) continue;

            Machine destinationMachine = FindDestinationForProduct(
                sourceMachine.machineType.outputProductType);

            if (destinationMachine != null && destinationMachine.CanAcceptInput(
                sourceMachine.machineType.outputProductType))
            {
                Logist logist = availableLogists[0];
                availableLogists.RemoveAt(0);

                TransportTask task = new TransportTask
                {
                    fromMachine = sourceMachine,
                    toMachine = destinationMachine,
                    productType = sourceMachine.machineType.outputProductType
                };

                logist.AssignTask(task);
                Debug.Log($"Назначена задача: {sourceMachine.machineType.displayName} -> {destinationMachine.machineType.displayName}");
            }
        }
    }

    private Machine FindDestinationForProduct(ProductType productType)
    {
        foreach (Machine machine in FindObjectsOfType<Machine>())
        {
            if (machine.machineType.inputProductType == productType &&
                machine.CanAcceptInput(productType))
                return machine;
        }
        return null;
    }
}