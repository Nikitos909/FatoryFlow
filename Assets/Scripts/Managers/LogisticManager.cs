using UnityEngine;

public class LogisticsManager : MonoBehaviour
{
    public List<Logist> availableLogists = new List<Logist>();
    private List<TransportTask> pendingTasks = new List<TransportTask>();
    
    public void OnProductProduced(Machine machine)
    {
        Machine destination = FindDestinationMachine(machine.machineType.outputProductType);
        if (destination != null)
        {
            // Автоматически определяем приоритет
            int priority = CalculatePriority(machine, destination);
            TransportTask task = new TransportTask(machine, destination, machine.machineType.outputProductType, priority);
            AddTask(task);
        }
    }
    
    private void AddTask(TransportTask task)
    {
        pendingTasks.Add(task);
        // Сортируем по приоритету и времени
        pendingTasks = pendingTasks
            .OrderBy(t => t.priority)
            .ThenBy(t => t.timestamp)
            .ToList();
            
        TryAssignTasks();
    }
    
    private int CalculatePriority(Machine source, Machine destination)
    {
        // Высокий приоритет если приемник простаивает
        if (!destination.isWorking && destination.currentInput == null) return 1;
        
        // Средний приоритет если продукт может заблокировать производство
        if (source.currentOutput != null) return 2;
        
        // Низкий приоритет для остальных случаев
        return 3;
    }
    
    private void TryAssignTasks()
    {
        // Обрабатываем задачи в порядке приоритета
        for (int i = 0; i < pendingTasks.Count && availableLogists.Count > 0; i++)
        {
            TransportTask task = pendingTasks[i];
            
            // Проверяем актуальность задачи
            if (task.sourceMachine.currentOutput == null || 
                !task.destinationMachine.CanAcceptInput(task.productType))
            {
                pendingTasks.RemoveAt(i);
                i--;
                continue;
            }
            
            Logist logist = availableLogists[0];
            availableLogists.RemoveAt(0);
            pendingTasks.RemoveAt(i);
            i--;
            
            logist.AssignTask(task.sourceMachine, task.destinationMachine);
        }
    }
}
