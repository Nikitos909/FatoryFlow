using UnityEngine;
using System.Collections.Generic;

public class SimpleLogisticsManager : MonoBehaviour
{
    public static SimpleLogisticsManager Instance;
    
    public List<SimpleLogist> availableLogists = new List<SimpleLogist>();
    private Queue<Machine> pendingTasks = new Queue<Machine>();

    vvoid Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Автоматически находим всех логистов на сцене
            SimpleLogist[] foundLogists = FindObjectsOfType<SimpleLogist>();
            availableLogists.AddRange(foundLogists);
            
            Debug.Log($"Логистический менеджер инициализирован. Найдено логистов: {availableLogists.Count}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

   public void OnProductCreated(Machine machine)
    {
        if (machine == null) return;
        
        pendingTasks.Enqueue(machine);
        Debug.Log($"Добавлена задача для станка: {machine.machineType.machineName}. В очереди: {pendingTasks.Count}");
        TryAssignTask();
    }

    public void OnTaskCompleted(SimpleLogist logist)
    {
        if (logist == null) return;
        
        if (!availableLogists.Contains(logist))
        {
            availableLogists.Add(logist);
            Debug.Log($"Логист свободен. Доступно логистов: {availableLogists.Count}");
        }
        TryAssignTask();
    }

     private void TryAssignTask()
    {
        if (pendingTasks.Count > 0 && availableLogists.Count > 0)
        {
            Machine sourceMachine = pendingTasks.Dequeue();
            if (sourceMachine == null) return;
            
            Machine destinationMachine = FindDestinationMachine(sourceMachine.machineType.outputProductType);
            
            if (destinationMachine != null)
            {
                SimpleLogist logist = availableLogists[0];
                availableLogists.RemoveAt(0);
                
                logist.AssignTask(sourceMachine, destinationMachine);
                Debug.Log($"Назначена задача: {sourceMachine.machineType.machineName} -> {destinationMachine.machineType.machineName}");
            }
            else
            {
                Debug.Log("Не найден подходящий станок-приемник. Задача возвращена в очередь");
                pendingTasks.Enqueue(sourceMachine); // Возвращаем в очередь если нет приемника
            }
        }
    }

     private Machine FindDestinationMachine(ProductType productType)
    {
        Machine[] machines = FindObjectsOfType<Machine>();
        foreach (Machine machine in machines)
        {
            if (machine != null && machine.CanAcceptInput(productType))
                return machine;
        }
        return null;
    }
}
