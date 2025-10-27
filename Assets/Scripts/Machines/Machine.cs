using UnityEngine;

public class Machine : MonoBehaviour
{
    public MachineTypeSO machineType;
    public Transform inputSlot;
    public Transform outputSlot;
    public Transform waitingPoint; // Точка ожидания для логиста

    public Product currentInput;
    public Product currentOutput;
    public bool isWorking = false;
    public float workTimer = 0f;

    private bool hasPendingOutput = false; 

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
        if (currentOutput != null && !HasActiveTask() && !hasPendingOutput)
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

        bool isDefective = Random.value < machineType.baseDefectChance;
        ProductType outputType = isDefective ? machineType.defectiveProductType : machineType.outputProductType;

        CreateOutputProduct(outputType, isDefective);
        
        // Уничтожаем входной продукт
        if (currentInput != null)
        {
            Destroy(currentInput.gameObject);
            currentInput = null;
        }

        Debug.Log($"{machineType.displayName} произвел {outputType}");
    }

    private void CreateOutputProduct(ProductType type, bool defective)
    {
        GameObject productObj = new GameObject($"Product_{type}");
        productObj.transform.position = outputSlot.position;

        Product product = productObj.AddComponent<Product>();
        product.Initialize(type, this, defective);

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
            hasPendingOutput = true;
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
                hasPendingOutput = true;
            }
            else
            {
                Debug.Log($"⏳ {machineType.displayName} ждет освобождения станка-приемника");
                // Запускаем корутину для повторной проверки
                StartCoroutine(RetryTransportTaskCreation());

            }
        }
    }

    private IEnumerator RetryTransportTaskCreation()
    {
        float waitTime = 2f;
        while (currentOutput != null && !HasActiveTask())
        {
            yield return new WaitForSeconds(waitTime);
            
            Machine destinationMachine = FindNextMachine();
            if (destinationMachine != null && destinationMachine.CanAcceptInput(machineType.outputProductType))
            {
                TransportTask task = new TransportTask(this, destinationMachine, machineType.outputProductType, 2);
                LogisticsManager.Instance.AddTask(task);
                hasPendingOutput = true;
                yield break;
            }
            else
            {
                Debug.Log($"🔄 {machineType.displayName} повторно проверяет приемник...");
            }
        }
    }

    private Machine FindNextMachine()
    {
        foreach (Machine machine in FindObjectsOfType<Machine>())
        {
            if (machine.machineType.inputProductType == machineType.outputProductType)
            {
                return machine;
            }
        }
        return null;
    }

    private bool HasActiveTask()
    {
        // Проверяем, есть ли уже задача для этого станка
        return LogisticsManager.Instance.HasTaskForMachine(this);
    }

    private Color GetProductColor(ProductType type)
    {
        return type switch
        {
            ProductType.RawPipe => Color.gray,
            ProductType.BentSector => Color.blue,
            ProductType.FinalProduct => Color.green,
            ProductType.DefectiveProduct => Color.red,
            _ => Color.white
        };
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

    public bool CanAcceptInput(ProductType type)
    {
        return !isWorking && currentInput == null && type == machineType.inputProductType;
    }

    public void PutInputProduct(Product product)
    {
        if (CanAcceptInput(product.type))
        {
            currentInput = product;
            product.transform.SetParent(inputSlot);
            product.transform.localPosition = Vector3.zero;
            Debug.Log($"{machineType.displayName} принял продукт: {product.type}");
        }
        else
        {
            Debug.LogWarning($"❌ {machineType.displayName} не может принять {product.type}");
        }
    }

    public Product TakeOutputProduct()
    {
        Product product = currentOutput;
        currentOutput = null;
        return product;
    }

    public Vector3 GetOutputSlotPosition()
    {
        return outputSlot.position;
    }

    public Vector3 GetWaitingPosition()
    {
        return waitingPoint != null ? waitingPoint.position : transform.position + Vector3.right * 2f;
    }

     public Product TakeOutputProduct()
    {
        Product product = currentOutput;
        currentOutput = null;
        hasPendingOutput = false; // Сбрасываем флаг при взятии продукта
        return product;
    }

    // Добавляем метод для уведомления о освобождении
    public void OnInputProcessed()
    {
        // Уведомляем менеджер логистики о том, что станок освободился
        if (LogisticsManager.Instance != null)
        {
            LogisticsManager.Instance.OnMachineBecameAvailable(this);
        }
    }
}
