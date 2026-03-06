using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RawMaterialWarehouse : MonoBehaviour
{
    public Transform spawnPoint;
    public int rawPipePrice = 50;
    public ProductType rawProductType = ProductType.RawPipe;

    private List<TransportTask> pendingRawMaterialTasks = new List<TransportTask>();

    // Список для отслеживания созданного сырья
    private List<Product> availableRawMaterials = new List<Product>();
    
    // Визуализация склада
    public SpriteRenderer warehouseSprite;
    
    void Start()
    {
        // Регистрируем склад в GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.warehouse = this;
        }
    }


//==============
    private void CreateRawProduct(ProductType type)
    {
        // Создаем объект из префаба
        GameObject productObj = Instantiate(machineType.inputProductPrefab, inputSlot.position, Quaternion.identity);
        productObj.name = $"Product_{type}";

        // Получаем компонент Product
        Product product = productObj.GetComponent<Product>();

        if (product != null)
        {
            product.Initialize(type, this);
            currentInput = product;
        }
    }
//=============
    
    // Вызывается по кнопке UI
    public bool BuyRawMaterial()
    {
        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendMoney(rawPipePrice))
        {
            if (newMaterial != null)
            {
                availableRawMaterials.Add(newMaterial);
                CreateTransportTask();
                return true;
            }
        }
        else
        {
            Debug.Log("❌ Недостаточно денег для покупки сырья!");
        }
        return false;
    }

    private void CreateTransportTask()
    {
        // Находим первый станок, который принимает сырье
        Machine destinationMachine = FindMachineForRawMaterial();
       
        if (destinationMachine != null)
        {
            TransportTask task = new TransportTask(
                sourceMachine: null, // null = склад
                dest: destinationMachine,
                type: rawProductType
            );
           
            if (LogisticsManager.Instance != null)
            {
                LogisticsManager.Instance.AddTask(task);
            }
        }
        else
        {
            Debug.LogWarning("❌ Не найден свободный станок для сырья!");
            // Создаем задачу и добавляем в отложенные
            TransportTask task = new TransportTask(null, null, rawProductType);
            StartCoroutine(RetryRawMaterialTask(task));
        }
    }

    private IEnumerator RetryRawMaterialTask(TransportTask task)
    {
        float waitTime = 3f;
        int maxAttempts = 10;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            yield return new WaitForSeconds(waitTime);
            
            Machine destinationMachine = FindMachineForRawMaterial();
            if (destinationMachine != null)
            {
                task.destinationMachine = destinationMachine;
                LogisticsManager.Instance.AddTask(task);
                yield break;
            }
            
            attempts++;
            Debug.Log($"🔄 Склад: повторная попытка найти станок ({attempts}/{maxAttempts})");
        }
        
        Debug.LogError("❌ Склад: не удалось найти станок после всех попыток!");
    }
    
    private Machine FindMachineForRawMaterial()
    {
        foreach (Machine machine in FindObjectsOfType<Machine>())
        {
            if (machine.machineType.inputProductType == rawProductType && 
                machine.CanAcceptInput(rawProductType))
            {
                return machine;
            }
        }
        return null;
    }

    // Метод для удаления сырья из списка при его взятии
    public void RemoveRawMaterial(Product product)
    {
        if (availableRawMaterials.Contains(product))
        {
            availableRawMaterials.Remove(product);
        }
    }

    // Получить доступное сырье
    public Product GetAvailableRawMaterial()
    {
        if (availableRawMaterials.Count > 0)
        {
            Product product = availableRawMaterials[0];
            availableRawMaterials.RemoveAt(0);
            return product;
        }
        return null;
    }

    public bool HasRawMaterialAvailable()
    {
        // Проверяем есть ли сырье на складе
        Product[] rawProducts = FindObjectsOfType<Product>();
        foreach (Product product in rawProducts)
        {
            if (product.type == ProductType.RawPipe && product.producedAt == null)
            {
                float distance = Vector3.Distance(product.transform.position, spawnPoint.position);
                if (distance < 3f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
