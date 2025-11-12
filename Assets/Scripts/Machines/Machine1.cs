using UnityEngine;
using UnityEngine.AddressableAssets;

public class Machine1 : MonoBehaviour
{
    public MachineTypeSO machineType;
    public Transform inputSlot;
    public Transform outputSlot;
    public Transform waitingPoint; // Точка ожидания для логиста

    public Product currentInput;
    public Product currentOutput;
    public bool isWorking = false;
    public float workTimer = 0f;

    void Update()
    {
        // ЕСЛИ есть входной продукт И нет выходного И не работаем - начинаем производство
        if (!isWorking && currentInput != null && currentOutput == null)
        {
            StartProduction();
        }
        
        if (isWorking)
        {
            workTimer -= Time.deltaTime;
            if (workTimer <= 0f) 
                FinishProduction();
        }

        // ЕСЛИ есть готовый продукт И нет активной задачи - создаем задачу
        if (currentOutput != null && !HasActiveTask())
        {
            CreateTransportTask();
        }
    }

    public void StartProduction()
    {
        isWorking = true;
        workTimer = machineType.baseProductionTime;
        Debug.Log($"{machineType.displayName} начал работу над {currentInput.type}");
    }

    private void FinishProduction()
    {
        isWorking = false;

        CreateOutputProduct(outputType);
        
        // Уничтожаем входной продукт
        if (currentInput != null)
        {
            Destroy(currentInput.gameObject);
            currentInput = null;
        }

        Debug.Log($"{machineType.displayName} произвел {outputType}");
    }
//=======================================
    private void CreateOutputProduct(ProductType type)
    {
        if (machineType.outputProductPrefab == null)
        {
            Debug.LogError($"Output prefab not set in {machineType.displayName}!");
            return;
        }
        // Создаем объект из префаба
        GameObject productObj = Instantiate(outputProduct, outputSlot.position, Quaternion.identity);
        productObj.name = $"Product_{type}";
    
        // Получаем компонент Product
        Product product = productObj.GetComponent<Product>();
        if (product != null)
        {
            product.Initialize(type, this);
            currentOutput = product;
        }

        //Addressables.InstantiateAsync("SektorOneChamferA", CNCMachine.positionCNCMachine,
       //         Quaternion.Euler(CNCMachine.angelCNCMachine.x, CNCMachine.angelCNCMachine.y + 90f, CNCMachine.angelCNCMachine.z)).Completed += handle =>
       //         {
       //             CNCMachine.details.Add(handle.Result);
      //          };
      //      Addressables.ReleaseInstance(lastSektor);
    }

    [SerializeField] private GameObject[] productPrefabs; // Настройте в инспекторе

private void CreateOutputProduct(ProductType type)
{
    GameObject prefab = GetPrefabByType(type);
    
    if (prefab == null)
    {
        Debug.LogError($"Prefab for {type} not found!");
        return;
    }

    GameObject productObj = Instantiate(prefab, outputSlot.position, Quaternion.identity);
    productObj.name = $"Product_{type}";

    Product product = productObj.GetComponent<Product>();
    product.Initialize(type, this);
    
    currentOutput = product;
}

private GameObject GetPrefabByType(ProductType type)
{
    foreach (var prefab in productPrefabs)
    {
        Product product = prefab.GetComponent<Product>();
        if (product != null && product.Type == type)
            return prefab;
    }
    return null;
}


//==================================================
    private void CreateOutputProduct(ProductType type)
    {
        GameObject productObj = new GameObject($"Product_{type}");
        productObj.transform.position = outputSlot.position;

        Product product = productObj.AddComponent<Product>();
        product.Initialize(type, this);

        SpriteRenderer sr = productObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateDefaultSprite();
        sr.color = GetProductColor(type);
        sr.sortingOrder = 1;

        // Добавляем коллайдер
        BoxCollider2D collider = productObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        currentOutput = product;
    }

    private void CreateTransportTask()
    {    
        // Определяем куда везти продукт
        Machine destinationMachine = null;
        
        // Если это ФИНАЛЬНЫЙ продукт - везем на склад продажи 
        if (machineType.outputProductType == ProductType.FinalProduct)
        {
            TransportTask task = new TransportTask(
                sourceMachine: this,
                dest: null, // null = склад продажи
                type: machineType.outputProductType,
                prio: 1
            );
            LogisticsManager.Instance.AddTask(task);
        }
        else
        {        
            // Для промежуточных продуктов ищем следующий станок
            destinationMachine = FindNextMachine();
            
            if (destinationMachine != null && destinationMachine.CanAcceptInput(machineType.outputProductType))
            {
                TransportTask task = new TransportTask(
                    sourceMachine: this,
                    dest: destinationMachine,
                    type: machineType.outputProductType,
                    prio: 2
                );
                LogisticsManager.Instance.AddTask(task);
            }
            else
            {
                Debug.Log($"⏳ {machineType.displayName} ждет освобождения станка-приемника");
                // Запускаем корутину для повторной проверки
                StartCoroutine(RetryTransportTaskCreation());

            }
        }
    }

    private bool HasActiveTask()
    {
        // Проверяем, есть ли уже задача для этого станка
        return LogisticsManager.Instance.HasTaskForMachine(this);
    }
}
