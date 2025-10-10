using UnityEngine;

public class RawMaterialWarehouse : MonoBehaviour
{
    public Transform spawnPoint;
    public int rawPipePrice = 50;
    public ProductType rawProductType = ProductType.RawPipe;

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
    
    private void SpawnRawMaterial()
    {
        GameObject rawMaterialObj = new GameObject($"RawMaterial_{Time.time}");
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
            
            LogisticsManager.Instance.AddTask(task);
        }
        else
        {
            Debug.LogWarning("❌ Не найден свободный станок для сырья!");
        }
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
    
    private Sprite CreateDefaultSprite()
    {
        Texture2D texture = new Texture2D(16, 16);
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                texture.SetPixel(x, y, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), Vector2.one * 0.5f);
    }
}
