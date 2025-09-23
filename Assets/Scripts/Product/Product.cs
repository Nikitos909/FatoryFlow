using UnityEngine;

public class Product : MonoBehaviour
{
    public ProductType type;
    public Machine producedAt;
    public bool isDefective;
    public int baseValue = 10;

    public void Initialize(ProductType productType, Machine machine, bool defective = false)
    {
        type = productType;
        producedAt = machine;
        isDefective = defective;
    }
}