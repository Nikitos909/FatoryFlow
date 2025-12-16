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
    public float workTimer = 50f;


    void StartProduction()
    {
        // ЕСЛИ есть входной продукт И нет выходного И не работаем - начинаем производство
        if (!isWorking && currentInput != null && currentOutput == null)
        {
            StartCoroutine(ProduceCoroutine());
        }
    }

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
        
        CreateTransportTask();
    }
//===================


    IEnumerator AssemblyProcess()
    {
        while (true)
        {
            if (!isWorking && currentInput != null && currentOutput == null)
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
                
                CreateTransportTask();
                yield return null;
            }
          
            else
            {
                _source.clip = _clipsWorks[Random.Range(0, _clipsWorks.Length - 1)];
                dataLevel.clockCycle = GetBuildDuration();
                _text.text = "Â ðàáîòå";
                timerImage.color = Color.green;
                _source.Play();
                StartCoroutine(Timer());
                yield return new WaitForSeconds(GetBuildDuration());
                _source.Stop();
                _source.clip = _changeover;
                _source.Play();
                _timeWork += GetBuildDuration();
                dataLevel.coefOfEquipment = _timeWork / MachineToolController.fullWorkTime * 100;
                dataLevel.quantityWarehouse = CalculateQuantityDetails(_recipientSlot, _slots);
                AddItemInSlotsOut(_recipientSlot);
                countDetails += 1;
                DecreaseItemInSlots(_slots);
                _text.text = "Ïåðåíàëàäêà";
                timerImage.color = Color.yellow;
                timerImage.fillAmount = 1f;
                yield return new WaitForSeconds(2f);
                _source.Stop();
            }
        }
    }


//==================    
    
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

    // Вызывается по кнопке UI Вспомогательный метод-временный
    public void BuyMaterial()
    {
        CreateInputProduct(machineType.inputProductType);
    }
}
