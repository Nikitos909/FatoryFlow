using UnityEngine;

public class Product : MonoBehaviour
{
    public ProductType type;
    public Machine producedAt;
    public bool isDefective;
    public int baseValue = 50;
    public bool isLockedForTransport = false; 

    public void Initialize(ProductType productType, Machine machine, bool defective = false)
    {
        type = productType;
        producedAt = machine;
        isDefective = defective;
        isLockedForTransport = false; 

        // Устанавливаем стоимость в зависимости от типа
        baseValue = type switch
        {
            ProductType.RawPipe => 10,
            ProductType.BentSector => 30,
            ProductType.FinalProduct => defective ? 5 : 100,
            _ => 10
        };
    }

    // Метод для блокировки коллайдера продукта во время траспортировки
    public void LockForTransport(Logist transporter)
    {
        isLockedForTransport = true;
        
        // Отключаем коллайдер на время транспортировки
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    // Метод для разблокировки коллайдера продукта после траспортировки
    public void UnlockAfterDelivery()
    {
        isLockedForTransport = false;
        
        // Включаем коллайдер обратно
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

}
