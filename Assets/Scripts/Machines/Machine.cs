using UnityEngine;
using System.Collections;

public class Machine1 : MonoBehaviour, ITaskGiver1
{
    [SerializeField] private MachineData1 machineData;
    [SerializeField] private Transform inputSlot;
    [SerializeField] private Transform outputSlot;
    [SerializeField] private GameObject processingParticles; // Визуальные эффекты для 2D

    private ProductType1 currentInputProduct;
    private GameObject currentInputVisual;
    private GameObject currentOutputVisual;
    private bool isProducing = false;
    private bool outputSlotFull = false;

    public bool CanAcceptProduct(ProductType1 product) => product == machineData.inputProductType;

    public bool IsInputSlotFree() => currentInputVisual == null && !isProducing;

    public void Initialize() 
    {
        Debug.Log($"Станок {machineData.machineName} инициализирован");
    }

    public void ReceiveProduct(ProductType1 product, GameObject productVisual = null)
    {
        if (!IsInputSlotFree() || !CanAcceptProduct(product)) 
        {
            Debug.Log($"Станок {machineData.machineName} не может принять продукт");
            return;
        }

        Debug.Log($"Станок {machineData.machineName} принял продукт {product}");

        currentInputProduct = product;
        currentInputVisual = productVisual;
        
        if (productVisual != null)
        {
            productVisual.transform.SetParent(inputSlot);
            productVisual.transform.localPosition = Vector3.zero;
        }

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
        
        // Визуальный эффект производства
        if (processingParticles != null)
            processingParticles.SetActive(true);

        Debug.Log($"Производство {machineData.outputProductType}...");
        yield return new WaitForSeconds(machineData.productionTime);

        // Завершение производства
        if (processingParticles != null)
            processingParticles.SetActive(false);

        // Очистка входного продукта
        if (currentInputVisual != null)
        {
            Destroy(currentInputVisual);
            currentInputVisual = null;
        }

        CreateOutputProduct();
        isProducing = false;

        // Автоматически запускаем следующее производство если есть сырье
        if (currentInputVisual != null)
        {
            StartProduction();
        }
    }

    private void CreateOutputProduct()
    {
        if (outputSlotFull) 
        {
            Debug.Log("Выходной слот занят, производство остановлено");
            return;
        }

        // Создаем визуал выходного продукта
        GameObject outputVisual = GameObject.CreatePrimitive(PrimitiveType.Quad); // Quad для 2D
        outputVisual.name = machineData.outputProductType.ToString();
        outputVisual.transform.SetParent(outputSlot);
        outputVisual.transform.localPosition = Vector3.zero;
        
        // Добавляем спрайт рендерер для 2D
        SpriteRenderer sr = outputVisual.GetComponent<SpriteRenderer>();
        if (sr == null) sr = outputVisual.AddComponent<SpriteRenderer>();
        
        // Меняем цвет в зависимости от типа продукта
        sr.color = GetProductColor(machineData.outputProductType);
        
        // Добавляем коллайдер для 2D
        BoxCollider2D collider = outputVisual.GetComponent<BoxCollider2D>();
        if (collider == null) outputVisual.AddComponent<BoxCollider2D>();

        currentOutputVisual = outputVisual;
        outputSlotFull = true;

        CreateTransportTaskForOutput();
    }

    private Color GetProductColor(ProductType1 product)
    {
        return product switch
        {
            ProductType.RawPipe => Color.gray,
            ProductType.CutPipe => Color.blue,
            ProductType.BentPipe => Color.red,
            _ => Color.white
        };
    }

    private void CreateTransportTaskForOutput()
    {
        ITaskGiver1 nextConsumer = FindNextConsumer(machineData.outputProductType);
        if (nextConsumer != null)
        {
            Vector3 fromPos = GetPosition();
            Vector3 toPos = nextConsumer.GetPosition();
            TransportTask1 task = new TransportTask1(machineData.outputProductType, fromPos, toPos, this);
            LogisticsManager1.Instance.AddTask(task);
            Debug.Log($"Задание на доставку {machineData.outputProductType} создано");
        }
        else
        {
            Debug.LogWarning($"Нет потребителя для {machineData.outputProductType}!");
        }
    }

    private ITaskGiver1 FindNextConsumer(ProductType1 product)
    {
        // Ищем станки
        Machine1[] machines = FindObjectsOfType<Machine1>();
        foreach (Machine1 machine in machines)
        {
            if (machine != this && machine.CanAcceptProduct(product) && machine.IsInputSlotFree())
            {
                return machine;
            }
        }

        // Ищем точку продажи
        ProductSellPoint1 sellPoint = FindObjectOfType<ProductSellPoint1>();
        if (sellPoint != null && sellPoint.CanAcceptProduct(product))
        {
            return sellPoint;
        }

        return null;
    }

    // ITaskGiver implementation
    public void OnTaskCompleted(TransportTask1 task)
    {
        if (task.ProductType == machineData.outputProductType)
        {
            outputSlotFull = false;
            if (currentOutputVisual != null)
            {
                Destroy(currentOutputVisual);
                currentOutputVisual = null;
            }
            Debug.Log($"Продукт {task.ProductType} забран со станка");
        }
    }

    public Vector3 GetPosition() => outputSlot != null ? outputSlot.position : transform.position;
}
