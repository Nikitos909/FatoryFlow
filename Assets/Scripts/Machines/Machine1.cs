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

}
