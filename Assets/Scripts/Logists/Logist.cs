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
        if (isEmployed)
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
        isDelivering = false;
        
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
           task.sourceMachine.outputSlot.position : 
           GetRawMaterialPosition();
        
        isDelivering = false;
    }

    private void PickUpProduct()
    {
        carriedProduct = currentTask.sourceMachine.currentOutput;
        carriedProduct.transform.SetParent(transform);
        carriedProduct.transform.localPosition = Vector3.up * 1.5f;
        if (currentTask.destinationMachine != null)
        {
            targetPosition = currentTask.destinationMachine.inputSlot.position;
        }
        else
        {
            Debug.Log("targetPosition of warehouse");
        }
        
        isDelivering = true;
    }

    private void  GetRawMaterialPosition()
    {
        Debug.Log("pick up from the warehouse");
    }

    private void DeliverProduct()
    {
        bool success = false;

        if (carriedProduct != null)
        {
            if (currentTask.destinationMachine != null)
            {
                Debug.Log("Find next Machine");
                // Доставляем на станок
                if (currentTask.destinationMachine.CanAcceptInput(carriedProduct.type))
                {
                    carriedProduct.UnlockAfterDelivery(); // Разблокировка перед отдачей на склад для отгрузки
                    //currentTask.destinationMachine.PutInputProduct(carriedProduct);
                    carriedProduct = null;
                    success = true;
                    Debug.Log($"📤 Логист {name} доставил на {currentTask.destinationMachine.machineType.displayName}");
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
    =============================================================*/
}
