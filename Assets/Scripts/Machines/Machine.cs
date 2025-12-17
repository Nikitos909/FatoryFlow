using UnityEngine;
using System.Collections;

public class Machine : MonoBehaviour
{
    public MachineTypeSO machineType;
    public Transform inputSlot;
    public Transform outputSlot;
    public Transform waitingPoint; // Точка ожидания для логиста

    public Product currentInput;
    public Product currentOutput;
    public bool isWorking = false;
    public float workTimer = 5f;


    private void Start()
    {
       TryStartProduction();
    }

        public void TryStartProduction()
    {
        // ЕСЛИ есть входной продукт И нет выходного И не работаем - начинаем производство
        if (!isWorking && currentInput != null && currentOutput == null)
        {
            StartCoroutine(ProduceCoroutine());
        }
    }

    //=========
    //private void Update()
    //{
    //    StartProduction();
    //}

   // void StartProduction()
   // {
        // ЕСЛИ есть входной продукт И нет выходного И не работаем - начинаем производство
    //    if (!isWorking && currentInput != null && currentOutput == null)
   //     {
    //        Debug.Log("work");
    //        StartCoroutine(ProduceCoroutine());
    //    }
  //  }

    //============

        public IEnumerator ProduceCoroutine()
    {
        isWorking = true;
        Debug.Log($"начал производство...");
       
        yield return new WaitForSeconds(workTimer);

        CreateOutputProduct(machineType.outputProductType);
        
        // Уничтожаем входной продукт
        if (currentInput != null)
        {
            Destroy(currentInput.gameObject);
            currentInput = null;
        }

        Debug.Log($"{machineType.displayName} произвел {machineType.outputProductType}");
        
        isWorking = false; // Важно: сбросить флаг работы
        
        // После завершения проверяем, можно ли начать новое производство
        TryStartProduction();
        
        CreateTransportTask();
    }

    // Вспомогательный метод- после отработки удалить
    private void CreateInputProduct(ProductType type)
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

        // Запускаем производство после создания продукта
        TryStartProduction();
    }
    
    // Вызывается по кнопке UI Вспомогательный метод-временный
    public void BuyMaterial()
    {
        CreateInputProduct(machineType.inputProductType);
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
        Machine destinationMachine = null;
        Debug.Log("Creating task");
        
        // Если это ФИНАЛЬНЫЙ продукт - везем на склад продажи 
        if (machineType.outputProductType == ProductType.FinalProduct)
        {
            TransportTask task = new TransportTask(
                sourceMachine: this,
                dest: null, // null = склад продажи
                type: machineType.outputProductType
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
                    type: machineType.outputProductType
                );
                LogisticsManager.Instance.AddTask(task);
            }
        }
        Debug.Log("Task created ==========>");
    }

    private bool HasActiveTask()
    {
        // Проверяем, есть ли уже задача для этого станка
        return LogisticsManager.Instance.HasTaskForMachine(this);
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

    public bool CanAcceptInput(ProductType type)
    {
        return !isWorking && currentInput == null && type == machineType.inputProductType;
    }
}
