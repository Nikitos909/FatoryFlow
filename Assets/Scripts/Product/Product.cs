using UnityEngine;

public class Product : MonoBehaviour
{
    public ProductType type;
    public Machine producedAt;
    public bool isDefective;
    public int baseValue = 50;

    public void Initialize(ProductType productType, Machine machine, bool defective = false)
    {
        type = productType;
        producedAt = machine;
        isDefective = defective;

        // Устанавливаем стоимость в зависимости от типа
        baseValue = type switch
        {
            ProductType.RawPipe => 10,
            ProductType.BentSector => 30,
            ProductType.FinalProduct => defective ? 5 : 100,
            _ => 10
        };
    }
}