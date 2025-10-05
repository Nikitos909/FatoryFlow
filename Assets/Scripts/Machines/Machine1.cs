using UnityEngine;
using System.Collections;

public class Machine1 : MonoBehaviour, ITaskGiver1
{
    [SerializeField] private MachineData1 machineData;
    [SerializeField] private Transform inputSlot; // Визуальный слот для входного продукта
    [SerializeField] private Transform outputSlot; // Визуальный слот для выходного продукта

    private ProductType1 currentInputProduct;
    private GameObject currentInputVisual;
    private GameObject currentOutputVisual;
    private bool isProducing = false;
    private bool outputSlotFull = false;

    public bool CanAcceptProduct(ProductType1 product) => product == machineData.inputProductType;

    public bool IsInputSlotFree() => currentInputVisual == null && !isProducing;

    public void Initialize() { /* ... */ }

    // Вызывается LogisticsManager, когда логист принес продукт
    public void ReceiveProduct(ProductType1 product, GameObject productVisual = null)
    {
        if (!IsInputSlotFree() || !CanAcceptProduct(product)) return;

        currentInputProduct = product;
        currentInputVisual = productVisual; // Логист "передает" визуальный объект станку
        if (productVisual != null)
        {
            // Размещаем визуал во входном слоте
            productVisual.transform.SetParent(inputSlot);
            productVisual.transform.localPosition = Vector3.zero;
        }

        // Начинаем производство
        StartProduction();
    }

    private void StartProduction()
    {
        if (isProducing) return;
        StartCoroutine(ProductionRoutine());
    }

    private IEnumerator ProductionRoutine()
    {
        isProducing = true;
        Debug.Log($"Станок {machineData.machineName} начал производство...");

        // Производственный цикл
        yield return new WaitForSeconds(machineData.productionTime);

        // Производство завершено
        Debug.Log($"Станок {machineData.machineName} произвел {machineData.outputProductType}");

        // Уничтожаем входной продукт (сырье переработано)
        if (currentInputVisual != null)
        {
            Destroy(currentInputVisual);
            currentInputVisual = null;
        }

        // Создаем выходной продукт
        CreateOutputProduct();

        isProducing = false;

        // Если можно, сразу запускаем следующую операцию (если на входе есть сырье)
        if (currentInputVisual != null)
        {
            StartProduction();
        }
    }

    private void CreateOutputProduct()
    {
        if (outputSlotFull) return;

        // Создаем визуал выходного продукта
        // (В реальной системе здесь может быть пул объектов или фабрика)
        GameObject outputVisual = GameObject.CreatePrimitive(PrimitiveType.Cube); // Заглушка
        outputVisual.transform.SetParent(outputSlot);
        outputVisual.transform.localPosition = Vector3.zero;
        outputVisual.name = machineData.outputProductType.ToString();
        currentOutputVisual = outputVisual;

        outputSlotFull = true;

        // Сообщаем LogisticsManager, что продукт готов к вывозу
        CreateTransportTaskForOutput();
    }

    private void CreateTransportTaskForOutput()
    {
        // Находим следующего потребителя для нашего продукта
        // (например, следующий станок в цепочке или точка продажи)
        ITaskGiver1 nextConsumer = FindNextConsumer(machineData.outputProductType);
        if (nextConsumer != null)
        {
            Vector3 fromPos = GetPosition();
            Vector3 toPos = nextConsumer.GetPosition();
            TransportTask1 task = new TransportTask1(machineData.outputProductType, fromPos, toPos, this);
            LogisticsManager1.Instance.AddTask(task);
        }
        else
        {
            Debug.LogWarning($"Нет потребителя для {machineData.outputProductType}!");
        }
    }

    private ITaskGiver1 FindNextConsumer(ProductType1 product)
    {
        // Упрощенная реализация. В реальной системе должен быть менеджер цепочек производства.
        // Ищем любой станок, который принимает наш выходной продукт как входной.
        Machine1[] allMachines = FindObjectsOfType<Machine1>();
        foreach (Machine1 machine in allMachines)
        {
            if (machine != this && machine.CanAcceptProduct(product) && machine.IsInputSlotFree())
            {
                return machine;
            }
        }

        // Если станков нет, может быть, это конечный продукт и его нужно отнести на склад/продажу?
        ProductSellPoint1 sellPoint = FindObjectOfType<ProductSellPoint1>();
        if (sellPoint != null && sellPoint.CanAcceptProduct(product))
        {
            return sellPoint;
        }

        return null;
    }

    // ITaskGiver1 implementation
    public void OnTaskCompleted(TransportTask1 task)
    {
        // Это вызывается, когда логист забрал наш выходной продукт
        if (task.ProductType == machineData.outputProductType)
        {
            // Освобождаем выходной слот
            outputSlotFull = false;
            currentOutputVisual = null;
            Debug.Log($"Продукт {task.ProductType} был забран со станка {machineData.machineName}");
        }
    }

    public Vector3 GetPosition() => outputSlot.position; // Логист подходит к выходному слоту
}
