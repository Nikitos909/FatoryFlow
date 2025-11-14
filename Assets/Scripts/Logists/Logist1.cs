using UnityEngine;
using System.Collections;

public class Logist1 : MonoBehaviour
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

     private void CompleteTask()
    {
        isMoving = false;
        isDelivering = false;
        isWaitingForProduct = false;
        currentTask = null;
        carriedProduct = null;
        pickupAttempts = 0;
       
        // Сообщаем менеджеру, что свободен
        if (LogisticsManager.Instance != null)
        {
            LogisticsManager.Instance.OnLogistAvailable(this);
        }
    }

}
