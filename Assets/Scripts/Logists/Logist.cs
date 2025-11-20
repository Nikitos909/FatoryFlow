using UnityEngine;
using System.Collections;

public class Logist : MonoBehaviour
{
    public float speed = 3f;
    public Product carriedProduct;

    private Vector3 targetPosition;
    private TransportTask currentTask;
    private bool isEmployed = false;
    private bool isDelivering = false;
    private Vector3 spawnPosition;

    void Start()
    {
        spawnPosition = transform.position;

        // Регистрируем в LogisticsManager
        if (LogisticsManager.Instance != null)
        {
            LogisticsManager.Instance.OnLogistAvailable(this);
        }
    }

    void Update()
    {
        if (!isEmployed)
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

    // Завершение задачи
    private void CompleteTask()
    {
        currentTask = null;
        isEmployed = false;
        
        // Сообщаем менеджеру о доступности
        if (LogisticsManager.Instance != null)
        {
            LogisticsManager.Instance.OnLogistAvailable(this);
        }
    }

    // Назначение задачи логисту
    public void AssignTask(TransportTask task)
    {
        currentTask = task;
        isEmployed = true;
        
        // Начинаем с подбора изделия
        targetPosition = task.sourceMachine != null ? 
            task.sourceMachine.GetPickupPosition() : 
            GetRawMaterialPosition();
        
        isDelivering = false;
    }

    private void PickUpProduct()
    {    
        Product productToPickup = null;

        // Берем продукт со станка или со склада
        if (currentTask.sourceMachine != null)
        {
            // Проверяем, есть ли готовый продукт на станке
            if (currentTask.sourceMachine.currentOutput != null)
            {
                productToPickup = currentTask.sourceMachine.TakeOutputProduct();
            }
            else
            {
                Debug.Log($"⏳ Логист {name}: продукт еще не готов на станке {currentTask.sourceMachine.machineType.displayName}");
                WaitForProduct();
                return;
            }
        }
        else
        {
            // Берем продукт со склада сырья
            productToPickup = FindRawMaterialOnWarehouse();
        }

        // Проверяем что продукт найден
        if (productToPickup != null)
        {
            carriedProduct = productToPickup;
            carriedProduct.LockForTransport(this); // Включается блокировка коллайдера при захвате
            carriedProduct.transform.SetParent(transform);
            carriedProduct.transform.localPosition = new Vector3(0, 0.5f, 0);

            // Определяем куда везти
            if (currentTask.destinationMachine != null)
            {
                targetPosition = currentTask.destinationMachine.GetWaitingPosition();
            }
            else
            {
                // Везем на склад продажи
                if (LogisticsManager.Instance != null && LogisticsManager.Instance.sellPoint != null)
                {
                    targetPosition = LogisticsManager.Instance.sellPoint.transform.position;
                }
                else
                {
                    Debug.LogError("❌ Точка продажи не найдена!");
                    CompleteTask();
                    return;
                }
            }

            isDelivering = true;
            isWaitingForProduct = false;
            Debug.Log($"📥 Логист {name} взял {carriedProduct.type}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Логист {name}: не нашел продукт для забора (попытка {pickupAttempts}/{MAX_PICKUP_ATTEMPTS})");        
        }
    }


    /*============================================================
     public float speed = 2f;
    public Product carriedProduct;

    private Vector3 targetPosition;
    private TransportTask currentTask;
    private bool isMoving = false;
    private bool isDelivering = false;
    private Vector3 spawnPosition;
    private bool isWaitingForProduct = false;

    void Start()
    {
        spawnPosition = transform.position;

        // Регистрируем в LogisticsManager
        if (LogisticsManager.Instance != null)
        {
            LogisticsManager.Instance.OnLogistAvailable(this);
        }
    }

    void Update()
    {
        if (isMoving && !isWaitingForProduct)
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
        pickupAttempts = 0;

        // Определяем начальную позицию для движения
        if (task.sourceMachine != null)
        {
            // Едем к станку-источнику
            targetPosition = task.sourceMachine.GetOutputSlotPosition();
        }
        else
        {
            // Едем на склад сырья
            if (GameManager.Instance != null && GameManager.Instance.warehouse != null && GameManager.Instance.warehouse.spawnPoint != null)
            {
                targetPosition = GameManager.Instance.warehouse.spawnPoint.position;
            }
            else
            {
                Debug.LogError("❌ Склад сырья не найден!");
                CompleteTask();
                return;
            }
        }

        isMoving = true;
        isDelivering = false;
        isWaitingForProduct = false;

        Debug.Log($"🚚 Логист {name} начал задачу: {task.productType}");
    }

    private void PickUpProduct()
    {
        // ДОБАВИТЬ ПРОВЕРКУ В НАЧАЛЕ МЕТОДА
        if (currentTask == null)
        {
            Debug.LogError($"❌ Логист {name}: безработный!");
            CompleteTask();
            return;
        }
    
        Product productToPickup = null;

        // Берем продукт со станка или со склада
        if (currentTask.sourceMachine != null)
        {
            // Проверяем, есть ли готовый продукт на станке
            if (currentTask.sourceMachine.currentOutput != null)
            {
                productToPickup = currentTask.sourceMachine.TakeOutputProduct();
            }
            else
            {
                Debug.Log($"⏳ Логист {name}: продукт еще не готов на станке {currentTask.sourceMachine.machineType.displayName}");
                WaitForProduct();
                return;
            }
        }
        else
        {
            // Берем продукт со склада сырья
            productToPickup = FindRawMaterialOnWarehouse();
        }

        // Проверяем что продукт найден
        if (productToPickup != null)
        {
            carriedProduct = productToPickup;
            carriedProduct.LockForTransport(this); // Включается блокировка коллайдера при захвате
            carriedProduct.transform.SetParent(transform);
            carriedProduct.transform.localPosition = new Vector3(0, 0.5f, 0);

            // Определяем куда везти
            if (currentTask.destinationMachine != null)
            {
                targetPosition = currentTask.destinationMachine.GetWaitingPosition();
            }
            else
            {
                // Везем на склад продажи
                if (LogisticsManager.Instance != null && LogisticsManager.Instance.sellPoint != null)
                {
                    targetPosition = LogisticsManager.Instance.sellPoint.transform.position;
                }
                else
                {
                    Debug.LogError("❌ Точка продажи не найдена!");
                    CompleteTask();
                    return;
                }
            }

            isDelivering = true;
            isWaitingForProduct = false;
            pickupAttempts = 0;
            Debug.Log($"📥 Логист {name} взял {carriedProduct.type}");
        }
        else
        {
            pickupAttempts++;
            Debug.LogWarning($"⚠️ Логист {name}: не нашел продукт для забора (попытка {pickupAttempts}/{MAX_PICKUP_ATTEMPTS})");

            if (pickupAttempts >= MAX_PICKUP_ATTEMPTS)
            {
                Debug.LogError($"❌ Логист {name}: не смог найти продукт после {MAX_PICKUP_ATTEMPTS} попыток, отменяем задачу");
                CompleteTask();
            }
            else
            {
                WaitForProduct();
            }
        }
    }

    private void WaitForProduct()
    {
        isWaitingForProduct = true;
        Debug.Log($"⏰ Логист {name} ждет продукт...");

        // Ждем 2 секунды и пробуем снова
        StartCoroutine(RetryPickupAfterDelay(2f));
    }

    private IEnumerator RetryPickupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isWaitingForProduct = false;
        PickUpProduct();
    }

    private Product FindRawMaterialOnWarehouse()
    {
        // Используем метод склада если доступен
        if (GameManager.Instance != null && GameManager.Instance.warehouse != null)
        {
            return GameManager.Instance.warehouse.GetAvailableRawMaterial();
        }

        // Резервный поиск
        Product[] rawProducts = FindObjectsOfType<Product>();
        foreach (Product product in rawProducts)
        {
            if (product.type == ProductType.RawPipe && product.producedAt == null)
            {
                // Проверяем, находится ли продукт в зоне склада
                if (GameManager.Instance != null && GameManager.Instance.warehouse != null)
                {
                    float distance = Vector3.Distance(product.transform.position,
                        GameManager.Instance.warehouse.spawnPoint.position);
                    if (distance < 3f) // Продукт находится рядом со складом
                    {
                        return product;
                    }
                }
            }
        }
        return null;
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
                    carriedProduct.UnlockAfterDelivery(); // Разблокировка перед отдачей на склад для отгрузки
                    currentTask.destinationMachine.PutInputProduct(carriedProduct);
                    carriedProduct = null;
                    success = true;
                    Debug.Log($"📤 Логист {name} доставил на {currentTask.destinationMachine.machineType.displayName}");
                }
                else
                {
                    Debug.LogWarning($"❌ Станок {currentTask.destinationMachine.machineType.displayName} занят, жду...");
                    StartCoroutine(RetryDeliveryAfterDelay(2f));
                    return;
                }
            }
            else
            {
                // Доставляем на склад продажи
                if (LogisticsManager.Instance != null && LogisticsManager.Instance.sellPoint != null)
                {
                    carriedProduct.UnlockAfterDelivery();
                    carriedProduct.transform.SetParent(null);
                    carriedProduct.transform.position = LogisticsManager.Instance.sellPoint.transform.position;

                    // Убедимся, что продукт активирован для продажи
                    if (carriedProduct.GetComponent<Collider2D>() == null)
                    {
                        carriedProduct.gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
                    }

                    carriedProduct = null;
                    success = true;
                    Debug.Log($"💰 Логист {name} доставил на склад продажи");
                }
            }
        }
        else
        {
            Debug.LogError($"❌ Логист {name}: пытается доставить null продукт!");
            CompleteTask();
            return;
        }

        if (success)
        {
            CompleteTask();
        }
    }

    private IEnumerator RetryDeliveryAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DeliverProduct();
    }

    // Визуализация для отладки
    void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = isDelivering ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, 0.2f);
        }
        
        if (isWaitingForProduct)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

    public void ReturnToSpawn()
    {
        if (!isMoving && !isWaitingForProduct)
        {
            if (LogisticsManager.Instance != null && LogisticsManager.Instance.logistSpawnPoint != null)
            {
                targetPosition = LogisticsManager.Instance.logistSpawnPoint.position;
                isMoving = true;
                isDelivering = false;
                Debug.Log($"🏠 Логист {name} возвращается на базу");
            }
        }
    }
    =============================================================*/
}
