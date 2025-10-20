using UnityEngine;

public class ProductSellPoint : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Product product = other.GetComponent<Product>();
        Debug.Log($"{product}");
        Debug.Log($"{product.type}");
        if (product != null && product.type == ProductType.FinalProduct)
        {
            // Продаем продукт
            EconomyManager.Instance.SellProduct(product);
            Destroy(other.gameObject);
            Debug.Log("✅ Продукт продан!");
            
            // Обновляем статистику
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnProductSold(product);
            }
        }
    }

    // Визуализация для отладки
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.8f);
    }
}
