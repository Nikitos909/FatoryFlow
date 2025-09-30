using UnityEngine;

public class Machine : MonoBehaviour
{
    public MachineTypeSO machineType;
    public Transform inputSlot;
    public Transform outputSlot;

    public Product currentInput;
    public Product currentOutput;
    public bool isWorking = false;
    public float workTimer = 0f;

   void Update()
    {
        if (isWorking)
        {
            workTimer -= Time.deltaTime;
            if (workTimer <= 0f) FinishProduction();
        }
        else if (currentInput != null && currentOutput == null)
        {
            StartProduction();
        }
        
        // ЕСЛИ есть готовый продукт И нет активной задачи - сообщаем логистике
        if (currentOutput != null && !HasTransportTask())
        {
            // Проверяем, чтобы не спамить
            if (Mathf.FloorToInt(Time.time) % 2 == 0) // Каждые 2 секунды
            {
                LogisticsManager.Instance.OnProductProduced(this);
                Debug.Log($"{machineType.displayName} ЕСТЬ ГОТОВЫЙ ПРОДУКТ");
            }
        }
    }

    public void StartProduction()
    {
        isWorking = true;
        workTimer = machineType.baseProductionTime;
        Debug.Log($"{machineType.displayName} начал работу");
    }

    private void FinishProduction()
    {
        isWorking = false;

        bool isDefective = Random.value < machineType.baseDefectChance;
        ProductType outputType = isDefective ? machineType.defectiveProductType : machineType.outputProductType;

        CreateOutputProduct(outputType, isDefective);
        Destroy(currentInput.gameObject);
        currentInput = null;

        LogisticsManager.Instance.OnProductProduced(this);
    }

    private void CreateOutputProduct(ProductType type, bool defective)
    {
        GameObject productObj = new GameObject($"Product_{type}");
        productObj.transform.position = outputSlot.position;

        Product product = productObj.AddComponent<Product>();
        product.Initialize(type, this, defective);

        SpriteRenderer sr = productObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateDefaultSprite();
        if (defective) sr.color = Color.red;
        else sr.color = GetProductColor(type);
        sr.sortingOrder = 1;

        currentOutput = product;
        Debug.Log($"{machineType.displayName} произвел {type}");
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
}
