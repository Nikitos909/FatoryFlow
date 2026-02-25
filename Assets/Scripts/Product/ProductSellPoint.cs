using UnityEngine;
using System.Collections;
using TMPro;

public class ProductSellPoint : MonoBehaviour
{
    public Transform sellPosition;
    public int quantityItem;  
    
    private int soldItemsCount = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        StartCoroutine(SellProducts(other.gameObject));
    }
    
    private IEnumerator SellProducts(GameObject productObject)
    {
        yield return new WaitForSeconds(2f);
        
        Product product = productObject.GetComponent<Product>();
        
        if (product != null && product.type == ProductType.FinalProduct)
        {
            // Продаем продукт
            EconomyManager.Instance.SellProduct(product);
            
            // Увеличиваем счетчики
            soldItemsCount++;
            quantityItem = soldItemsCount;  
            
            Debug.Log($"✅ Продукт продан! Всего продано: {soldItemsCount}");
            
            Destroy(productObject);
            
            // Обновляем статистику
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnProductSold(product);
            }
        }
    }
}
