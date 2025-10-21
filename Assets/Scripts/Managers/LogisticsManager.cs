using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LogisticsManager : MonoBehaviour
{
    public static LogisticsManager Instance;
    
    public List<Logist> allLogists = new List<Logist>();
    public List<Logist> availableLogists = new List<Logist>();
    private Queue<TransportTask> taskQueue = new Queue<TransportTask>();
    public ProductSellPoint sellPoint;
    public Transform logistSpawnPoint; // Начальная точка для логистов

    // Для отложенных задач
    private List<TransportTask> pendingTasks = new List<TransportTask>(); // Отложенные задачи
    private float retryTimer = 0f;
    private const float RETRY_INTERVAL = 2f; // Проверять каждые 2 секунды


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

    void Start()
    {
        // Находим все объекты
        if (sellPoint == null)
            sellPoint = FindObjectOfType<ProductSellPoint>();
            
        if (logistSpawnPoint == null)
        {
            GameObject spawnPoint = new GameObject("LogistSpawnPoint");
            logistSpawnPoint = spawnPoint.transform;
            logistSpawnPoint.position = Vector3.zero;
        }
        
        // Находим всех логистов на сцене
        allLogists = new List<Logist>(FindObjectsOfType<Logist>());
        availableLogists = new List<Logist>(allLogists);
        
        Debug.Log($"🚚 Инициализировано логистов: {allLogists.Count}");
        
        // Отправляем всех логистов на стартовую позицию
        foreach (Logist logist in allLogists)
        {
            logist.ReturnToSpawn();
        }
    }

    void FixedUpdate()
    {
        // Периодически проверяем отложенные задачи
        retryTimer -= Time.deltaTime;
        if (retryTimer <= 0f)
        {
            RetryPendingTasks();
            retryTimer = RETRY_INTERVAL;
        }
    }

    private void RetryPendingTasks()
    {
        if (pendingTasks.Count == 0) return;

        // Пробуем добавить отложенные задачи обратно в очередь
        for (int i = pendingTasks.Count - 1; i >= 0; i--)
        {
            TransportTask task = pendingTasks[i];
            if (IsTaskValid(task))
            {
                pendingTasks.RemoveAt(i);
                AddTask(task);
                Debug.Log($"🔄 Повторно добавляем задачу: {task.productType}");
            }
        }
    }

    // ДОБАВЛЕНИЕ задачи в очередь
    public void AddTask(TransportTask task)
    {
        taskQueue.Enqueue(task);
        Debug.Log($"✅ Добавлена задача: {task.productType} -> " +
                 $"{(task.destinationMachine != null ? task.destinationMachine.machineType.displayName : "СКЛАД")} " +
                 $"(очередь: {taskQueue.Count})");

        TryAssignTask();
    }

    // ПОПЫТКА назначить задачу свободному логисту
    private void TryAssignTask()
    {
        if (taskQueue.Count == 0 || availableLogists.Count == 0) 
            return;

        // Берем первую задачу из очереди
        TransportTask task = taskQueue.Peek();
        
        // Проверяем валидность задачи
        if (!IsTaskValid(task))
        {
            Debug.Log($"⏳ Задача отложена: {task.productType} (станок занят)");
            taskQueue.Dequeue();

            // Добавляем в отложенные вместо удаления
            if (!pendingTasks.Contains(task))
            {
                pendingTasks.Add(task);
            }

            TryAssignTask(); // Пробуем следующую задачу
            return;
        }

        // Назначаем задачу первому свободному логисту
        Logist logist = availableLogists[0];
        availableLogists.RemoveAt(0);
        taskQueue.Dequeue();
        
        logist.AssignTask(task);
        Debug.Log($"🎯 Задача назначена логисту {logist.name}: {task.productType}");
        
        // Пробуем назначить следующую задачу (если есть свободные логисты)
        if (availableLogists.Count > 0 && taskQueue.Count > 0)
        {
            TryAssignTask();
        }
    }

    // Добавляем метод для принудительной проверки при освобождении станка
    public void OnMachineBecameAvailable(Machine machine)
    {
        Debug.Log($"🔄 Станок {machine.machineType.displayName} освободился, проверяем отложенные задачи");
        RetryPendingTasks();
    }

    private bool IsTaskValid(TransportTask task)
    {
        // Для задач со складом сырья (source = null) - проверяем наличие сырья
        if (task.sourceMachine == null)
        {
            // Проверяем, есть ли сырье на складе
            if (GameManager.Instance != null && GameManager.Instance.warehouse != null)
            {
                return GameManager.Instance.warehouse.HasRawMaterialAvailable();
            }
            return false;
        }

        // Проверяем существование станка-источника
        if (task.sourceMachine == null)
            return false;

        // Проверяем наличие продукта
        if (task.sourceMachine.currentOutput == null)
            return false;

        // Для задач на продажу проверяем только наличие продукта
        if (task.destinationMachine == null)
            return true;

        // Для обычных задач проверяем, что приемник может принять продукт
        return task.destinationMachine.CanAcceptInput(task.productType);
    }

    // ЛОГИСТ освободился
    public void OnLogistAvailable(Logist logist)
    {
        if (!availableLogists.Contains(logist))
        {
            availableLogists.Add(logist);
            Debug.Log($"✅ Логист {logist.name} свободен");
        }
        
        // Пытаемся дать ему новую задачу
        TryAssignTask();
        
        // Если задач нет - возвращаем на базу
        if (taskQueue.Count == 0)
        {
            logist.ReturnToSpawn();
        }
    }

    public bool HasTaskForMachine(Machine machine)
    {
        return taskQueue.Any(task => task.sourceMachine == machine);
    }

    // Методы для UI и отладки
    public int GetQueueCount() => taskQueue.Count;
    public int GetAvailableLogistsCount() => availableLogists.Count;
    
    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 300, 100), "Логистика");
        GUI.Label(new Rect(20, 40, 280, 20), $"Очередь задач: {taskQueue.Count}");
        GUI.Label(new Rect(20, 60, 280, 20), $"Свободных логистов: {availableLogists.Count}");
        GUI.Label(new Rect(20, 80, 280, 20), $"Всего логистов: {allLogists.Count}");
    }
}
