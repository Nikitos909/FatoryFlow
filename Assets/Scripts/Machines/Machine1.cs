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

        CreateOutputProduct(outputType);
        
        // Уничтожаем входной продукт
        if (currentInput != null)
        {
            Destroy(currentInput.gameObject);
            currentInput = null;
        }

        Debug.Log($"{machineType.displayName} произвел {outputType}");
    }

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
}
