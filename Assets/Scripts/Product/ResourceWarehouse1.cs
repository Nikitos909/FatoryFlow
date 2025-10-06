using UnityEngine;

public class ResourceWarehouse1 : MonoBehaviour, ITaskGiver1
{
    [SerializeField] private ProductType1 storedProductType = ProductType1.RawPipe;
    [SerializeField] private int pipeCost = 100;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject rawPipePrefab; // Префаб сырой трубы для 2D

    public void Initialize() 
    {
        Debug.Log("Склад инициализирован");
    }

    // Кнопка UI должна вызывать этот метод
    public void BuyRawMaterial()
    {
        Debug.Log("Попытка купить сырье...");
        
        if (EconomyManager1.Instance.SpendMoney(pipeCost))
        {
            Debug.Log("Сырье куплено!");
            CreateTransportTask();
        }
    }

    private void CreateTransportTask()
    {
        // Создаем визуал сырья на складе
        CreateProductVisual();

        // Ищем первый свободный станок, который принимает сырые трубы
        Machine1 targetMachine = FindFirstMachineThatNeeds(storedProductType);
        
        if (targetMachine != null)
        {
            Vector3 fromPos = GetPosition();
            Vector3 toPos = targetMachine.GetPosition();
            TransportTask1 task = new TransportTask(storedProductType, fromPos, toPos, this);
            LogisticsManager1.Instance.AddTask(task);
            Debug.Log($"Задание создано: со склада в {targetMachine.name}");
        }
        else
        {
            Debug.LogWarning("Нет свободных станков для обработки сырья!");
        }
    }

    private Machine1 FindFirstMachineThatNeeds(ProductType1 product)
    {
        Machine1[] allMachines = FindObjectsOfType<Machine1>();
        foreach (Machine1 machine in allMachines)
        {
            if (machine.CanAcceptProduct(product) && machine.IsInputSlotFree())
            {
                return machine;
            }
        }
        return null;
    }

    private void CreateProductVisual()
    {
        if (rawPipePrefab != null && spawnPoint != null)
        {
            GameObject product = Instantiate(rawPipePrefab, spawnPoint.position, Quaternion.identity);
            product.name = $"RawPipe_{System.Guid.NewGuid()}";
        }
        else
        {
            Debug.LogWarning("Не назначен prefab или spawnPoint для сырья!");
        }
    }

    // ITaskGiver implementation
    public void OnTaskCompleted(TransportTask1 task)
    {
        Debug.Log($"Сырье доставлено со склада на станок");
    }

    public Vector3 GetPosition() 
    {
        return spawnPoint != null ? spawnPoint.position : transform.position;
    }
}
