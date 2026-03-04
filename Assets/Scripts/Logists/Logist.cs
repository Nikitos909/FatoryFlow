using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Logist : MonoBehaviour
{
    public float speed = 3f;
    public Product carriedProduct;

    private Vector3 targetPosition;
    private TransportTask currentTask;
    private bool isEmployed = false;
    public bool isDelivering = false;
    private Vector3 spawnPosition;

    private bool isSearchingForMachine = false;

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
           FindObjectOfType<ProductSellPoint>().sellPosition.position;
        
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
                FindObjectOfType<ProductSellPoint>().sellPosition.position;

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

    private void SetDeliveryTarget()
    {
        // Устанавливаем цель доставки
        targetPosition = currentTask.destinationMachine != null ?
                currentTask.destinationMachine.inputSlot.position :
                FindObjectOfType<ProductSellPoint>().sellPosition.position;
   
        isDelivering = true;
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
                else
                {
                    Debug.Log($"🔍 Логист {name} подошел к станку {currentTask.destinationMachine.name}, но он занят. Ищу другой...");
                
                    // Станок занят - ищем другой свободный
                    Machine alternativeMachine = FindFreeMachineForProduct(carriedProduct.type);

                    if (alternativeMachine != null)
                    {
                        // Нашли альтернативный станок
                        Debug.Log($"✅ Логист {name} нашел альтернативный станок {alternativeMachine.name}");
                        currentTask.destinationMachine = alternativeMachine;
                        SetDeliveryTarget();
                        return; // Отправляемся к новому станку
                    }
                    else
                    {
                        // Нет свободных станков - начинаем поиск
                        Debug.Log($"⚠️ Логист {name} не нашел альтернативный станок, начинаю поиск...");
                        StartCoroutine(SearchForFreeMachine());
                        return;
                    }

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
        Debug.Log(success);
        if (success)
        {
            CompleteTask();
        }
    }

    // НОВАЯ КОРУТИНА: поиск свободного станка
    private IEnumerator SearchForFreeMachine()
    {
        isSearchingForMachine = true;
        
        float searchTime = 0f;
        float maxSearchTime = 10f; // Максимальное время поиска (чтобы не зависнуть навсегда)
        float checkInterval = 0.5f; // Проверяем каждые 0.5 секунды
        
        Debug.Log($"🔍 Логист {name} начал поиск свободного станка для {carriedProduct.type}");
        
        while (searchTime < maxSearchTime)
        {
            // Ищем свободный станок
            Machine freeMachine = FindFreeMachineForProduct(carriedProduct.type);
            
            if (freeMachine != null)
            {
                Debug.Log($"✅ Логист {name} нашел свободный станок {freeMachine.name} после {searchTime:F1} сек");
                currentTask.destinationMachine = freeMachine;
                isSearchingForMachine = false;
                SetDeliveryTarget();
                yield break;
            }
            
            // Ждем перед следующей проверкой
            searchTime += checkInterval;
            yield return new WaitForSeconds(checkInterval);
        }
        
        // Если время поиска истекло, а станок не найден
        Debug.LogWarning($"⚠️ Логист {name} не нашел свободный станок за {maxSearchTime} сек. Отмена задачи.");
        
        // Возвращаем продукт обратно на исходный станок
        if (carriedProduct != null)
        {
            currentTask.sourceMachine.currentOutput = carriedProduct;
            carriedProduct.transform.SetParent(null);
            carriedProduct.transform.position = currentTask.sourceMachine.outputSlot.position;
            carriedProduct = null;
        }
        
        isSearchingForMachine = false;
        CompleteTask();
    }
}
