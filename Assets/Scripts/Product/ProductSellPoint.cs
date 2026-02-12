using UnityEngine;

public class ProductSellPoint : MonoBehaviour
{
    public Transform sellPosition;
    public int quantityItem;

    void OnTriggerEnter2D(Collider2D other)
    {
        StartCoroutine(SellfProducts());
    }
    
    private IEnumerator SellProducts()
    {
        yield return new WaitForSeconds(2f);
        
        Product product = other.GetComponent<Product>();

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
}
