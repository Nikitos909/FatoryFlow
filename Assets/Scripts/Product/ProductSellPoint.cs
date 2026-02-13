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
        StartCoroutine(SellProduct(other.gameObject));
    }
    
        private IEnumerator SellProduct(GameObject productObject)
    {
        yield return new WaitForSeconds(2f);
        
        Product product = productObject.GetComponent<Product>();
        
        if (product != null && product.type == ProductType.FinalProduct)
        {
            // Продаем продукт
            EconomyManager.Instance.SellProduct(product);
            
            // Увеличиваем счетчики
            soldItemsCount++;
            totalItemsSold++;
            
            // Обновляем отображение счетчика
            UpdateCounterDisplay();
            
            Destroy(productObject);
            Debug.Log($"✅ Продукт продан! Всего продано: {soldItemsCount}");
            
            // Обновляем статистику
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnProductSold(product);
            }
        }
    }

    // Метод для обновления отображения счетчика
    private void UpdateCounterDisplay()
    {
        string counterTextValue = $"Продано: {soldItemsCount}";
        
        // Обновляем UI текст
        if (counterText != null)
        {
            counterText.text = counterTextValue;
        }
        
        // Обновляем World Space текст
        if (counterWorldText != null)
        {
            counterWorldText.text = counterTextValue;
        }
    }
}
