using UnityEngine;
using System.Collections.Generic;

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
        if (!isEmployed) return;        
        MoveToTarget();
    }
    
    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        
        if (carriedProduct != null)
        {
            carriedProduct.transform.position = transform.position + Vector3.up * 1.1f;
        }
        
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (!isDelivering)
                PickUpProduct();
            else
                DeliverProduct();
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
        currentTask.sourceMachine.currentOutput = null;
        carriedProduct.transform.SetParent(null);

        currentTask.destinationMachine = FindFreeMachineForProduct(carriedProduct.type);

        // Устанавливаем цель доставки
        targetPosition = currentTask.destinationMachine != null ?
                currentTask.destinationMachine.inputSlot.position : 
                GetRawMaterialPosition();
    
        isDelivering = true;
    }

    private Machine FindFreeMachineForProduct(ProductType productType)
    {
        List<Machine> allMachines = new List<Machine>(FindObjectsOfType<Machine>());
        List<Machine> suitableMachines = new List<Machine>();
        
        // Ищем все станки, которые могут принять этот тип продукта
        foreach (Machine machine in allMachines)
        {
            // Пропускаем станок-источник
            if (machine == currentTask.sourceMachine)
                continue;
                
            // Проверяем, что станок может принимать этот тип продукта
            if (machine.machineType.inputProductType == productType)
            {
                Debug.Log($"{machine.name} check productType");
                // Проверяем, свободен ли станок (не занят работой и нет входного продукта)
                if (machine.CanAcceptInput(productType))
                {
                    Debug.Log($"{machine.name} are free");
                    suitableMachines.Add(machine);
                }
            }
        }
        Debug.Log(suitableMachines.Count);
        // Если нашли подходящие свободные станки
        if (suitableMachines.Count > 0)
        {
            return suitableMachines[0];
        }

        return null;
    }

    private Vector3 GetRawMaterialPosition()
    {
        return Vector3.zero;
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
                    carriedProduct.transform.SetParent(null);
                    currentTask.destinationMachine.PutInputProduct(carriedProduct);
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
        =============================================================*/
    }
