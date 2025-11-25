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
    
    // Вызывается по кнопке UI
    public bool BuyRawMaterial()
    {
        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendMoney(rawPipePrice))
        {
            Product newMaterial = SpawnRawMaterial();
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

    private Product SpawnRawMaterial()
    {
        GameObject rawMaterialObj = new GameObject($"RawMaterial_{System.DateTime.Now.Ticks}");
        rawMaterialObj.transform.position = spawnPoint.position;

        Product product = rawMaterialObj.AddComponent<Product>();
        product.Initialize(rawProductType, null);

        SpriteRenderer sr = rawMaterialObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateDefaultSprite();
        sr.color = Color.gray;
        sr.sortingOrder = 1;

        // Добавляем коллайдер для логиста
        BoxCollider2D collider = rawMaterialObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Debug.Log("📦 Создано сырье на складе");
        return product;
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
                type: rawProductType,
                prio: 1
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
            TransportTask task = new TransportTask(null, null, rawProductType, 1);
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

    private Sprite CreateDefaultSprite()
    {
        Texture2D texture = new Texture2D(16, 16);
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                texture.SetPixel(x, y, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), Vector2.one * 0.5f);
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
