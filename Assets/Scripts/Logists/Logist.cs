using UnityEngine;

public class Logist : MonoBehaviour
{
    public float speed = 2f;
    public Product carriedProduct;

    private Vector3 targetPosition;
    private TransportTask currentTask;
    private bool isMoving = false;
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
       
        // Определяем начальную позицию для движения
        if (task.sourceMachine != null)
        {
            // Едем к станку-источнику
            targetPosition = task.sourceMachine.GetOutputSlotPosition();
        }
        else
        {
            // Едем на склад сырья
            if (GameManager.Instance != null && GameManager.Instance.warehouse != null)
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
       
        Debug.Log($"🚚 Логист {name} начал задачу: {task.productType}");
    }

    private void PickUpProduct()
    {
        Product productToPickup = null;
       
        // Берем продукт со станка или со склада
        if (currentTask.sourceMachine != null)
        {
            productToPickup = currentTask.sourceMachine.TakeOutputProduct();
        }
        else
        {
            // Берем продукт со склада сырья
            productToPickup = FindRawMaterialOnWarehouse();
        }

        if (productToPickup != null)
        {
            carriedProduct = productToPickup;
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
            Debug.Log($"📥 Логист {name} взял {carriedProduct.type}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Логист {name}: не нашел продукт для забора");
            // Ждем немного и пробуем снова
            Invoke("RetryPickup", 1f);
        }
    }

        private Product FindRawMaterialOnWarehouse()
    {
        // Ищем сырье на складе
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
                    if (distance < 2f) // Продукт находится рядом со складом
                    {
                        return product;
                    }
                }
            }
        }
        return null;
    }

    private void RetryPickup()
    {
        PickUpProduct();
    }

    p    private void DeliverProduct()
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
                    Debug.Log($"📤 Логист {name} доставил на {currentTask.destinationMachine.machineType.displayName}");
                }
                else
                {
                    Debug.LogWarning($"❌ Станок {currentTask.destinationMachine.machineType.displayName} занят, оставляю продукт у себя");
                    // Ждем и пробуем снова
                    Invoke("RetryDelivery", 1f);
                    return;
                }
            }
            else
            {
                // Доставляем на склад продажи
                if (LogisticsManager.Instance != null && LogisticsManager.Instance.sellPoint != null)
                {
                    carriedProduct.transform.SetParent(null);
                    carriedProduct.transform.position = LogisticsManager.Instance.sellPoint.transform.position;
                    
                    // Убедимся, что продукт активирован для продажи
                    if (carriedProduct.GetComponent<Collider2D>() == null)
                    {
                        carriedProduct.gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
                    }
                    
                    carriedProduct = null;
                    success = true;
                    Debug.Log($"💰 Логист {name} доставил на СКЛАД");
                }
            }
        }

        if (success)
        {
            CompleteTask();
        }
    }

    private void RetryDelivery()
    {
        DeliverProduct();
    }

   private void CompleteTask()
    {
        isMoving = false;
        isDelivering = false;
        currentTask = null;
        carriedProduct = null;
       
        // Сообщаем менеджеру, что свободен
        if (LogisticsManager.Instance != null)
        {
            LogisticsManager.Instance.OnLogistAvailable(this);
        }
    }

   // Визуализация для отладки
    void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = isDelivering ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }

    public void ReturnToSpawn()
    {
        if (!isMoving)
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
}
