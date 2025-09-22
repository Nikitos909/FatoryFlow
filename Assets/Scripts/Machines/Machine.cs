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
    }

    private void FinishProduction()
    {
        isWorking = false;
        progressBar.gameObject.SetActive(false);

        // Создаем продукт
        bool isDefective = Random.value < defectChance;
        ProductType outputType = isDefective ? machineType.defectiveProductType : machineType.outputProductType;

        CreateOutputProduct(outputType, isDefective);
        Destroy(currentInput.gameObject);
        currentInput = null;

        // Сообщаем о новом продукте
        LogisticsManager.Instance.OnProductCreated(this);
    }

    private void CreateOutputProduct(ProductType type, bool defective)
    {
        GameObject productObj = new GameObject($"Product_{type}");
        productObj.transform.position = outputSlot.position;
        productObj.transform.SetParent(transform);

        Product product = productObj.AddComponent<Product>();
        product.Initialize(type, this, defective);

        SpriteRenderer sr = productObj.AddComponent<SpriteRenderer>();
        sr.sprite = GetProductSprite(type);
        if (defective) sr.color = Color.red;

        currentOutput = product;
    }

    private Sprite GetProductSprite(ProductType type)
    {
        // Заглушка - в реальности брать из конфига
        return Resources.Load<Sprite>("Sprites/" + type.ToString());
    }

    public bool CanAcceptInput(ProductType type)
    {
        return !isBroken && !isWorking && currentInput == null && type == machineType.inputProductType;
    }

    public void SetInputProduct(Product product)
    {
        if (CanAcceptInput(product.type))
        {
            currentInput = product;
            product.transform.SetParent(inputSlot);
            product.transform.localPosition = Vector3.zero;
        }
    }

    public Product TakeOutputProduct()
    {
        Product product = currentOutput;
        currentOutput = null;
        return product;
    }

    public Vector3 GetOutputPosition()
    {
        return outputSlot.position;
    }
}
