using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class LogisticsManager : MonoBehaviour
{
    public static LogisticsManager Instance;
    
    public List<Logist> allLogists = new List<Logist>();
    public List<Logist> availableLogists = new List<Logist>();
    private Queue<TransportTask> taskQueue = new Queue<TransportTask>();
    public ProductSellPoint sellPoint;

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
        
        // Находим всех логистов на сцене
        allLogists = new List<Logist>(FindObjectsOfType<Logist>());
        availableLogists = new List<Logist>(allLogists);
    }

    private void LateUpdate()
    {
        if (availableLogists.Count > 0 && (taskQueue.Count > 0))
        {
            Debug.Log("TryAssignTask()");
            TryAssignTask();
        }
        Debug.Log(taskQueue.Count);
    }

    // ДОБАВЛЕНИЕ задачи в очередь
    public void AddTask(TransportTask task)
    {
        taskQueue.Enqueue(task);
        // Небольшая задержка для синхронизации
        StartCoroutine(DelayedTryAssignTask());
    }

    private IEnumerator DelayedTryAssignTask()
    {
        yield return new WaitForEndOfFrame();
         if (availableLogists.Count > 0)
        {
            TryAssignTask();
        }
    }

    // ПОПЫТКА назначить задачу свободному логисту
    private void TryAssignTask()
    {
        if (taskQueue.Count == 0 || availableLogists.Count == 0) 
            return;

        // Берем первую задачу из очереди
        TransportTask task = taskQueue.Peek();
        
        // Назначаем задачу первому свободному логисту
        Logist logist = availableLogists[0];
        availableLogists.RemoveAt(0);
        taskQueue.Dequeue();
        
        logist.AssignTask(task);
    }

    
//======= Thinking about this method
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
        }
        
        // Пытаемся дать ему новую задачу
        TryAssignTask();
    }

    public bool HasTaskForMachine(Machine machine)
    {
        return taskQueue.Any(task => task.sourceMachine == machine);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 40, 280, 20), $"Очередь задач: {taskQueue.Count}");
        GUI.Label(new Rect(20, 60, 280, 20), $"Свободных логистов: {availableLogists.Count}");
        // Заголовок
        GUI.Label(new Rect(20, 80, 280, 20), $"Очередь задач ({taskQueue.Count}):");
        
        // Выводим все элементы очереди
        int index = 0;
        foreach (var task in taskQueue)
        {
            GUI.Label(new Rect(20, 100 + (index + 1) * 20, 500, 20), 
                     $"{index + 1}. {task}");
            index++;
        }

    }
}
