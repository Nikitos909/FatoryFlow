using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machine : MonoBehaviour
{
    public MachineTypeSO machineType;
    public Transform inputSlot;
    public Transform outputSlot;
    public ProgressBar progressBar;

    [Header("Текущее состояние")]
    public Product currentInput;
    public Product currentOutput;
    public bool isWorking = false;
    public bool isBroken = false;
    public float workTimer = 0f;

    private float productionTime;
    private float defectChance;

    void Start()
    {
        productionTime = machineType.baseProductionTime;
        defectChance = machineType.baseDefectChance;
    }

    void Update()
    {
        if (isBroken) return;
        
        if (isWorking)
        {
            workTimer -= Time.deltaTime;
            progressBar.SetProgress(1f - (workTimer / productionTime));
            
            if (workTimer <= 0f)
            {
                FinishProduction();
            }
        }
        else if (currentInput != null && currentOutput == null)
        {
            StartProduction();
        }
    }

     public void StartProduction()
    {
        isWorking = true;
        workTimer = productionTime;
        progressBar.gameObject.SetActive(true);
        Debug.Log($"{machineType.displayName} начал производство");
    }
   
    private void FinishProduction()
    {
        isWorking = false;
        
        // Определяем тип продукта (брак или норма)
        bool isDefective = Random.value < machineType.baseDefectChance;
        ProductType outputType = isDefective ? 
            machineType.defectiveProductType : machineType.outputProductType;
        
        // Создаем продукт
        CreateOutputProduct(outputType, isDefective);
        
        // Уничтожаем входной продукт
        Destroy(currentInput.gameObject);
        currentInput = null;
        
        // Сообщаем логистике о новом продукте
        LogisticsManager.Instance.OnProductProduced(this);
    }

    private void CreateOutputProduct(ProductType type, bool defective)
    {
        GameObject productObj = new GameObject($"Product_{type}");
        productObj.transform.position = outputSlot.position;
        
        Product product = productObj.AddComponent<Product>();
        product.Initialize(type, this, defective);
        
        // Добавляем визуал
        SpriteRenderer sr = productObj.AddComponent<SpriteRenderer>();
        sr.sprite = GetProductSprite(type);
        if (defective) sr.color = Color.red;
        
        currentOutput = product;
        Debug.Log($"{machineType.displayName} произвел: {type}" + 
                 (defective ? " (БРАК)" : ""));
    }

    private Sprite GetProductSprite(ProductType type)
    {
        // Простая заглушка - в реальном проекте брать из ресурсов
        return Resources.Load<Sprite>("Sprites/" + type.ToString());
    }

    public bool CanAcceptInput(ProductType type)
    {
        return !isBroken && !isWorking && currentInput == null && type == machineType.inputProductType;
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

    public Vector3 GetInputSlotPosition()
    {
        return inputSlot.position;
    }
}
