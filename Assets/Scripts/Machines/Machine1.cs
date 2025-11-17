using UnityEngine;

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

        CreateOutputProduct(machineType.outputProductType);
        
        // Уничтожаем входной продукт
        if (currentInput != null)
        {
            Destroy(currentInput.gameObject);
            currentInput = null;
        }

        Debug.Log($"{machineType.displayName} произвел {machineType.outputProductType}");
    }

    private void CreateOutputProduct(ProductType type)
    {
        if (machineType.outputProductPrefab == null)
        {
            Debug.LogError($"Output prefab not set in {machineType.displayName}!");
            return;
        }
        // Создаем объект из префаба
        GameObject productObj = Instantiate(machineType.outputProductPrefab, outputSlot.position, Quaternion.identity);
        productObj.name = $"Product_{type}";
    
        // Получаем компонент Product
        Product product = productObj.GetComponent<Product>();
        if (product != null)
        {
            product.Initialize(type, this);
            currentOutput = product;
        }
    }

    private void CreateTransportTask()
    {    
        // Определяем куда везти продукт
        Machine1 destinationMachine = null;
        
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
            //destinationMachine = FindNextMachine();
            
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
                //StartCoroutine(RetryTransportTaskCreation());

            }
        }
    }

    private bool HasActiveTask()
    {
        // Проверяем, есть ли уже задача для этого станка
        return LogisticsManager.Instance.HasTaskForMachine(this);
    }

    private Machine1 FindNextMachine()
    {
        foreach (Machine1 machine in FindObjectsOfType<Machine1>())
        {
            if (machine.machineType.inputProductType == machineType.outputProductType)
            {
                return machine;
            }
        }
        return null;
    }

    public bool CanAcceptInput(ProductType type)
    {
        return !isWorking && currentInput == null && type == machineType.inputProductType;
    }
}
