using UnityEngine;

public class Logist : MonoBehaviour
{
    public float speed = 2f;
    public Product carriedProduct;

    private Vector3 targetPosition;
    private TransportTask currentTask;
    private bool isMoving = false;
    private bool isDelivering = false;

    void Start()
    {
        // Автоматически регистрируемся в LogisticsManager
        if (LogisticsManager.Instance != null && !LogisticsManager.Instance.availableLogists.Contains(this))
        {
            LogisticsManager.Instance.availableLogists.Add(this);
            Debug.Log($"🚚 Логист {name} зарегистрирован");
        }
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                if (!isDelivering)
                    PickUpProduct();
                else
                    DeliverProduct();
            }
        }
    }

    public void AssignTask(TransportTask task)
    {
        currentTask = task;

        if (task.sourceMachine == null)
        {
            Debug.LogError("❌ ОШИБКА: sourceMachine is null!");
            CompleteTask();
            return;
        }

        targetPosition = task.sourceMachine.GetOutputSlotPosition();
        isMoving = true;
        isDelivering = false;

        Debug.Log($"🚚 Логист {name} получил задачу: {task.productType}");
    }

    private void PickUpProduct()
    {
        if (currentTask == null || currentTask.sourceMachine == null)
        {
            Debug.LogError("❌ ОШИБКА: currentTask или sourceMachine is null в PickUpProduct!");
            CompleteTask();
            return;
        }

        carriedProduct = currentTask.sourceMachine.TakeOutputProduct();
        if (carriedProduct != null)
        {
            carriedProduct.transform.SetParent(transform);
            carriedProduct.transform.localPosition = new Vector3(0, 0.5f, 0);

            // Определяем куда двигаться дальше
            if (currentTask.destinationMachine != null)
            {
                // Обычная задача: на другой станок
                targetPosition = currentTask.destinationMachine.transform.position;
                Debug.Log($"📥 Логист {name} взял {carriedProduct.type}, везу на {currentTask.destinationMachine.machineType.displayName}");
            }
            else
            {
                // Задача на продажу: на точку продажи
                if (LogisticsManager.Instance.sellPoint != null)
                {
                    targetPosition = LogisticsManager.Instance.sellPoint.transform.position;
                    Debug.Log($"📥 Логист {name} взял {carriedProduct.type}, везу на СКЛАД");
                }
                else
                {
                    Debug.LogError("❌ Нет точки продажи!");
                    CompleteTask();
                    return;
                }
            }

            isDelivering = true;
        }
        else
        {
            Debug.LogWarning($"⚠️ Логист {name}: продукт исчез, отмена задачи");
            CompleteTask();
        }
    }

    private void DeliverProduct()
    {
        bool success = false;

        if (carriedProduct != null)
        {
            if (currentTask.destinationMachine != null)
            {
                // Доставляем на станок
                if (currentTask.destinationMachine.CanAcceptInput(carriedProduct.type))
                {
                    currentTask.destinationMachine.PutInputProduct(carriedProduct);
                    carriedProduct = null;
                    success = true;
                    Debug.Log($"📤 Логист {name} доставил продукт на {currentTask.destinationMachine.machineType.displayName}");
                }
                else
                {
                    Debug.LogWarning($"❌ Логист {name}: станок {currentTask.destinationMachine.machineType.displayName} не может принять продукт");
                    // Оставляем продукт у себя и завершаем задачу
                    carriedProduct.transform.SetParent(null);
                }
            }
            else
            {
                // Доставляем на склад (продажа)
                // Product будет уничтожен в ProductSellPoint.OnTriggerEnter2D
                carriedProduct.transform.SetParent(null);

                // Позиционируем продукт рядом с точкой продажи
                if (LogisticsManager.Instance.sellPoint != null)
                {
                    carriedProduct.transform.position = LogisticsManager.Instance.sellPoint.transform.position;
                    Debug.Log($"💰 Логист {name} доставил продукт на СКЛАД");
                }

                carriedProduct = null;
                success = true;
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Логист {name}: нет продукта для доставки");
        }

        CompleteTask();
    }

    private void CompleteTask()
    {
        isMoving = false;
        isDelivering = false;
        currentTask = null;

        // ВАЖНО: Всегда сообщаем о завершении задачи
        if (LogisticsManager.Instance != null)
        {
            LogisticsManager.Instance.OnTaskCompleted(this);
        }
        else
        {
            Debug.LogError("❌ LogisticsManager.Instance is null!");
        }

        Debug.Log($"✅ Логист {name} завершил задачу и свободен");
    }

    // Визуализация для отладки
    void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = isDelivering ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, targetPosition);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
        else if (LogisticsManager.Instance != null && LogisticsManager.Instance.availableLogists.Contains(this))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }
    }
}
