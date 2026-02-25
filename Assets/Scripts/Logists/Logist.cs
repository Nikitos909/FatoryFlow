using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Logist : MonoBehaviour
{
    public float speed = 3f;
    public Product carriedProduct;

    private Vector3 targetPosition;
    private TransportTask currentTask;
    private bool isEmployed = false;
    public bool isDelivering = false;
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

    void FixedUpdate()
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
        // Проверяем, что источник существует
        if (currentTask.sourceMachine == null)
        {
            CompleteTask();
            return;
        }
        
        // Проверяем, что продукт существует
        if (currentTask.sourceMachine.currentOutput == null)
        {
            Debug.LogError($"❌ На станке {currentTask.sourceMachine.machineType.displayName} нет продукта для подбора!");
            
            // Ждем немного и проверяем снова
            StartCoroutine(WaitAndRetryPickup());
            return;
        }
        
        carriedProduct = currentTask.sourceMachine.currentOutput;
        currentTask.sourceMachine.currentOutput = null;
        carriedProduct.transform.SetParent(null);

        currentTask.destinationMachine = FindFreeMachineForProduct(carriedProduct.type);

        if (currentTask.destinationMachine == null && currentTask.productType != ProductType.FinalProduct)
        {
            Debug.LogWarning($"⚠️ Не найден свободный станок для {carriedProduct.type}");
            // Можно вернуть продукт обратно или ждать
        }


        // Устанавливаем цель доставки
        targetPosition = currentTask.destinationMachine != null ?
                currentTask.destinationMachine.inputSlot.position : 
                GetRawMaterialPosition();
    
        isDelivering = true;
    }

    private IEnumerator WaitAndRetryPickup()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (currentTask.sourceMachine.currentOutput != null)
        {
            PickUpProduct();
        }
        else
        {
            Debug.LogError("❌ Продукт так и не появился, отменяем задачу");
            CompleteTask();
        }
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
                // Проверяем, свободен ли станок (не занят работой и нет входного продукта)
                if (machine.CanAcceptInput(productType))
                {
                    suitableMachines.Add(machine);
                }
            }
        }

        // Если нашли подходящие свободные станки
        if (suitableMachines.Count > 0)
        {
            return suitableMachines[0];
        }
        return null;
    }

    private Vector3 GetRawMaterialPosition()
    {
        Transform sellPoint = FindObjectOfType<ProductSellPoint>().sellPoint;
        return sellPoint.position;
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
                    //carriedProduct.transform.position = LogisticsManager.Instance.sellPoint.sellPosition.position;

                    carriedProduct = null;
                    success = true;
                    Debug.Log($"💰 Логист {name} доставил на склад продажи");
                }
            }
        }
        else
        {
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
