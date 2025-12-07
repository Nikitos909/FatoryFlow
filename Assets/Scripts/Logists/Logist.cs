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
        
        Debug.Log("Logist recived task");
        
        // Начинаем с подбора изделия
        //targetPosition = task.sourceMachine != null ? 
         //   task.sourceMachine.GetPickupPosition() : 
         //   GetRawMaterialPosition();
        
        isDelivering = false;
    }

        // Возврат на точку спавна
    public void ReturnToSpawn()
    {
        targetPosition = spawnPosition;
        isEmployed = true;
        isDelivering = false;
    }

    private void PickUpProduct()
    {
        //CurrentProduct = CurrentTask.product;
        //CurrentProduct.transform.SetParent(transform);
        //CurrentProduct.transform.localPosition = Vector3.up * 1.5f;
    }

    private void DeliverProduct()
    {
        
    }
    /*============================================================
   
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
