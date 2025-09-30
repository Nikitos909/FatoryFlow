using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LogisticsManager : MonoBehaviour
{
    public static LogisticsManager Instance;

    public List<Logist> availableLogists = new List<Logist>();
    private List<TransportTask> pendingTasks = new List<TransportTask>();
    public ProductSellPoint sellPoint;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("Initialize Logistics Manager");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (sellPoint == null)
            sellPoint = FindObjectOfType<ProductSellPoint>();

        // Автоматически находим всех логистов на сцене
        availableLogists = new List<Logist>(FindObjectsOfType<Logist>());
        Debug.Log($"Найдено логистов: {availableLogists.Count}");
    }

    if (productType == ProductType.FinalProduct)
        {
            if (sellPoint != null)
            {
                TransportTask task = new TransportTask(
                    machine, 
                    null, // null = склад
                    productType, 
                    1 // Высокий приоритет
                );
                AddTask(task);
                Debug.Log($"💰 Создана задача на ПРОДАЖУ: {productType}");
            }
        }
        // ВСЕ промежуточные продукты идут на следующие станки
        else
        {
            Machine destination = FindDestinationMachine(productType);
            if (destination != null)
            {
                // ПРОВЕРЯЕМ, может ли станок принять продукт прямо сейчас
                if (destination.CanAcceptInput(productType))
                {
                    int priority = CalculatePriority(machine, destination);
                    TransportTask task = new TransportTask(machine, destination, productType, priority);
                    AddTask(task);
                    Debug.Log($"🔄 Создана задача на ПЕРЕМЕЩЕНИЕ: {productType} → {destination.machineType.displayName}");
                }
                else
                {
                    // Станок занят - ЖДЕМ его освобождения
                    Debug.Log($"⏳ Станок {destination.machineType.displayName} занят, ждем освобождения для {productType}");
                    // Задача НЕ создается - продукт останется на текущем станке
                    // Логист будет периодически проверять возможность доставки
                }
            }
            else
            {
                Debug.LogWarning($"❌ Не найден станок-приемник для {productType}");
                // НЕ создаем задачу на продажу для промежуточных продуктов!
            }
        }
        
        // После создания задачи проверяем, нет ли "зависших" продуктов
        CheckForBlockedProduction();
    }

    // Проверяем блокировки производства
    private void CheckForBlockedProduction()
    {
        // Ищем станки, у которых есть продукт, но нет задачи
        foreach (Machine machine in FindObjectsOfType<Machine>())
        {
            if (machine.currentOutput != null && !HasTaskForMachine(machine))
            {
                // Пытаемся создать задачу для "зависшего" продукта
                TryCreateTaskForBlockedProduct(machine);
            }
        }
    }

    private bool HasTaskForMachine(Machine machine)
    {
        return pendingTasks.Any(task => task.sourceMachine == machine);
    }

    private void TryCreateTaskForBlockedProduct(Machine machine)
    {
        ProductType productType = machine.currentOutput.type;
        
        if (productType == ProductType.FinalProduct)
        {
            // Финальный продукт всегда можно отвезти на склад
            if (sellPoint != null)
            {
                TransportTask task = new TransportTask(machine, null, productType, 1);
                AddTask(task);
                Debug.Log($"🔄 Создана задача для ЗАВИСШЕГО продукта: {productType} → СКЛАД");
            }
        }
        else
        {
            // Для промежуточного продукта ищем свободный станок
            Machine destination = FindDestinationMachine(productType);
            if (destination != null && destination.CanAcceptInput(productType))
            {
                TransportTask task = new TransportTask(machine, destination, productType, 1); // Высокий приоритет
                AddTask(task);
                Debug.Log($"🔄 Создана задача для ЗАВИСШЕГО продукта: {productType} → {destination.machineType.displayName}");
            }
        }
    }

    public void OnTaskCompleted(Logist logist)
    {
        // ВАЖНО: Всегда возвращаем логиста в доступные
        if (!availableLogists.Contains(logist))
        {
            availableLogists.Add(logist);
            Debug.Log($"✅ Логист {logist.name} вернулся в доступные");
        }
        
        // После завершения задачи проверяем блокировки
        CheckForBlockedProduction();
        TryAssignTasks();

    }

    private void TryAssignTasks()
    {
        if (pendingTasks.Count == 0 || availableLogists.Count == 0) 
            return;

        Debug.Log($"🎯 Распределяю {pendingTasks.Count} задач между {availableLogists.Count} логистами");

        for (int i = pendingTasks.Count - 1; i >= 0; i--)
        {
            if (availableLogists.Count == 0) break;

            TransportTask task = pendingTasks[i];
            
            if (!IsTaskValid(task))
            {
                Debug.Log($"🗑️ Задача устарела: {task.productType}");
                pendingTasks.RemoveAt(i);
                continue;
            }

            Logist logist = availableLogists[0];
            availableLogists.RemoveAt(0);
            pendingTasks.RemoveAt(i);

            logist.AssignTask(task);
            Debug.Log($"🎯 Задача назначена: {task.productType} → {logist.name}");
        }
    }

   private bool IsTaskValid(TransportTask task)
    {
        if (task.sourceMachine == null || task.sourceMachine.currentOutput == null)
            return false;

        // Для задач на продажу проверяем только наличие продукта
        if (task.destinationMachine == null)
            return true;

        // Для обычных задач проверяем, что приемник может принять продукт
        return task.destinationMachine.CanAcceptInput(task.productType);
    }

    private void AddTask(TransportTask task)
    {
        if (pendingTasks.Any(t => t.sourceMachine == task.sourceMachine && t.productType == task.productType))
        {
            Debug.Log($"⚠️ Задача уже существует: {task.productType} от {task.sourceMachine.machineType.displayName}");
            return;
        }

        pendingTasks.Add(task);
        
        pendingTasks = pendingTasks
            .OrderBy(t => t.priority)
            .ThenBy(t => t.timestamp)
            .ToList();
        
        Debug.Log($"✅ Добавлена задача: {task.productType} (приоритет: {task.priority}), всего задач: {pendingTasks.Count}");
        
        TryAssignTasks();
    }

   private Machine FindDestinationMachine(ProductType productType)
    {
        // Ищем станок, который принимает этот тип продукта
        foreach (Machine machine in FindObjectsOfType<Machine>())
        {
            if (machine.machineType.inputProductType == productType)
            {
                return machine;
            }
        }
        
        return null;
    }

    private int CalculatePriority(Machine source, Machine destination)
    {
        // Высокий приоритет если приемник простаивает
        if (!destination.isWorking && destination.currentInput == null) 
            return 1;
        
        // Средний приоритет если продукт блокирует производство
        if (source.currentOutput != null) 
            return 2;
        
        return 3;
    }

    // Принудительная проверка всех станков
    public void CheckAllMachinesForTasks()
    {
        Debug.Log("🔍 Принудительная проверка всех станков на задачи...");
        
        foreach (Machine machine in FindObjectsOfType<Machine>())
        {
            if (machine.currentOutput != null && !HasTaskForMachine(machine))
            {
                OnProductProduced(machine);
            }
        }
    }

    // Метод для отладки - принудительно создает задачу
    public void DebugCreateTask()
    {
        Machine machineWithProduct = FindObjectsOfType<Machine>()
            .FirstOrDefault(m => m.currentOutput != null);

        if (machineWithProduct != null)
        {
            OnProductProduced(machineWithProduct);
        }
        else
        {
            Debug.Log("❌ Нет станков с продуктами для отладки");
        }
    }

    // В LogisticsManager добавьте метод для тестирования
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 30), "Debug: Create Task"))
        {
            DebugCreateTask();
        }

        if (GUI.Button(new Rect(10, 50, 200, 30), "Debug: Show Status"))
        {
            Debug.Log($"Логистов: {availableLogists.Count}, Задач: {pendingTasks.Count}");
        }
    }
}
