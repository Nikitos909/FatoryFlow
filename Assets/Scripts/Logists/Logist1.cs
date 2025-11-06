using UnityEngine;
using System.Collections;

public class Logist : MonoBehaviour
{
    public float speed = 2f;
    public Product carriedProduct;

    private Vector3 targetPosition;
    private TransportTask currentTask;
    private bool isMoving = false;
    private bool isDelivering = false;
    private Vector3 spawnPosition;
    private bool isWaitingForProduct = false;
    private int pickupAttempts = 0;
    private const int MAX_PICKUP_ATTEMPTS = 3;

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
                targetPosition = currentTas
