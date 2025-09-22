using System.Collections.Generic;
using UnityEngine;

public class LogisticManager : MonoBehaviour
{
    public static LogisticManager Instance;

    public List<Logist> AllLogists = new List<Logist>();
    public List<TransportTask> PendingTasks = new List<TransportTask>();


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

    public void Initialize(float baseSpeed)
    {
        foreach (var logist in AllLogists)
            logist.Initialize(baseSpeed);
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var logist in AllLogists)
        {
            if (logist.isSick) continue;
            
            logist.UpdateLogic(deltaTime);
            
            if (logist.CurrentTask == null && PendingTasks.Count > 0)
                AssignTaskToLogist(logist, PendingTasks[0]);
        }
    }
   public void OnProductsReady(List<MachineSlot> outputSlots)
    {
        foreach (var slot in outputSlots)
        {
            if (slot.CurrentProduct != null)
            {
                // Создаем задание на перемещение
                var task = new TransportTask(
                    product: slot.CurrentProduct,
                    fromMachine: slot.Machine,
                    toMachine: FindDestinationMachine(slot.CurrentProduct.Type)
                );
                PendingTasks.Add(task);
            }
        }
    }

    private void AssignTaskToLogist(Logist logist, TransportTask task)
    {
        logist.AssignTask(task);
        PendingTasks.Remove(task);
    }

    private Machine FindDestinationMachine(ProductType productType)
    {
        // Ищем станок, которому нужен этот продукт
        foreach (var machine in ProductionManager.Instance.AllMachines)
        {
            if (machine.inputSlots.Any(slot => slot.RequiredType == productType))
                return machine;
        }
        return null; // Или склад
    }
}
