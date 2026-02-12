using UnityEngine;
using System.Collections;

public class ProductSellPoint : MonoBehaviour
{
    public Transform sellPosition;

    // Добавляем UI компоненты для отображения счетчика
    public TextMeshProUGUI counterText; // Для UI Canvas
    public TextMesh counterWorldText;   // Для World Space текста (3D Text)
    public GameObject counterPanel;     // Панель для отображения счетчика
    
    private int soldItemsCount = 0;
    private int totalItemsSold = 0;     // Общее количество проданных предметов за всю игру

    void OnTriggerEnter2D(Collider2D other)
    {
        StartCoroutine(SellfProducts(other.gameObject));
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
            quantityItem = soldItemsCount; // Обновляем публичное поле для доступа из других скриптов
            
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
