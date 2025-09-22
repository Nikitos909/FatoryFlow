using UnityEngine;

public class Product : MonoBehaviour
{
    using UnityEngine;

public class Product : MonoBehaviour
{
    [Header("Идентификация")]
    public ProductType type; // Тип продукта
    public string productName; // Имя для отображения
    public int baseValue; // Базовая стоимость

    [Header("Состояние")]
    public bool isDefective = false; // Бракованный ли продукт
    public Machine producedAt; // Станок, где был произведен
    public float productionTime; // Время производства

    [Header("Визуал")]
    public Sprite sprite; // Спрайт для отображения
    public Color defectiveColor = Color.red; // Цвет для брака

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    public void Initialize(ProductType type, Machine productionMachine, bool defective = false)
    {
        this.type = type;
        this.producedAt = productionMachine;
        this.isDefective = defective;
        this.productionTime = Time.time;

        // Настройка в зависимости от типа
        switch (type)
        {
            case ProductType.RawPipe:
                productName = "Сырая труба";
                baseValue = 50;
                break;
            case ProductType.BentSector:
                productName = "Гнутый сектор";
                baseValue = 100;
                break;
            case ProductType.FinalProduct:
                productName = isDefective ? "Бракованный отвод" : "Готовый отвод";
                baseValue = isDefective ? 10 : 200;
                break;
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
            if (isDefective)
            {
                spriteRenderer.color = defectiveColor;
            }
        }
    }

    // Метод для утилизации (возвращает стоимость лома)
    public int GetScrapValue()
    {
        return isDefective ? baseValue / 2 : 0;
    }

    public ProductData GetData()
    {
        return new ProductData
        {
            type = type,
            isDefective = isDefective,
            position = transform.position,
            rotation = transform.rotation
        };
    }
}

}
