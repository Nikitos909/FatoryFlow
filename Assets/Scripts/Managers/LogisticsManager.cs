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

    public void OnTaskCompleted(Logist logist)
    {
        // ВАЖНО: Всегда возвращаем логиста в доступные
        if (!availableLogists.Contains(logist))
        {
            availableLogists.Add(logist);
            Debug.Log($"✅ Логист {logist.name} вернулся в доступные");
        }

        // Пытаемся сразу дать новую задачу
        TryAssignTasks();
    }

    private void TryAssignTasks()
    {
        if (pendingTasks.Count == 0)
        {
            Debug.Log("📭 Нет задач в очереди");
            return;
        }

        if (availableLogists.Count == 0)
        {
            Debug.Log("👥 Нет свободных логистов");
            return;
        }

        Debug.Log($"🎯 Пытаюсь распределить {pendingTasks.Count} задач между {availableLogists.Count} логистами");

        // Обрабатываем задачи пока есть логисты
        for (int i = pendingTasks.Count - 1; i >= 0; i--)
        {
            if (availableLogists.Count == 0) break;

            TransportTask task = pendingTasks[i];

            // Проверяем актуальность задачи
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

        // Отладочная информация
        if (pendingTasks.Count > 0)
        {
            Debug.Log($"📋 Осталось задач в очереди: {pendingTasks.Count}");
        }
    }

    private bool IsTaskValid(TransportTask task)
    {
        // Проверяем существование станка-источника
        if (task.sourceMachine == null)
        {
            Debug.LogWarning("❌ Задача невалидна: sourceMachine уничтожен");
            return false;
        }

        // Проверяем наличие продукта
        if (task.sourceMachine.currentOutput == null)
        {
            Debug.LogWarning("❌ Задача невалидна: продукт исчез");
            return false;
        }

        // Для задач на продажу - только наличие продукта
        if (task.destinationMachine == null)
        {
            return true; // Всегда валидна для продажи
        }

        // Для обычных задач проверяем приемник
        if (task.destinationMachine == null)
        {
            Debug.LogWarning("❌ Задача невалидна: destinationMachine уничтожен");
            return false;
        }

        return task.destinationMachine.CanAcceptInput(task.productType);
    }

    private void AddTask(TransportTask task)
    {
        // Проверяем дубликаты
        if (pendingTasks.Any(t => t.sourceMachine == task.sourceMachine && t.productType == task.productType))
        {
            Debug.Log($"⚠️ Задача уже существует: {task.productType} от {task.sourceMachine.machineType.displayName}");
            return;
        }

        pendingTasks.Add(task);

        // Сортируем по приоритету и времени
        pendingTasks = pendingTasks
            .OrderBy(t => t.priority)
            .ThenBy(t => t.timestamp)
            .ToList();

        Debug.Log($"✅ Добавлена задача: {task.productType} (приоритет: {task.priority}), всего задач: {pendingTasks.Count}");

        // Пытаемся сразу назначить
        TryAssignTasks();
    }

    private Machine FindDestinationMachine(ProductType productType)
    {
        Machine[] allMachines = FindObjectsOfType<Machine>();

        foreach (Machine machine in allMachines)
        {
            // Ищем станок, который принимает этот тип продукта И может принять прямо сейчас
            if (machine.machineType.inputProductType == productType &&
                machine.CanAcceptInput(productType))
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

        // Средний приоритет если продукт может заблокировать производство
        if (source.currentOutput != null)
            return 2;

        // Низкий приоритет для остальных случаев
        return 3;
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
