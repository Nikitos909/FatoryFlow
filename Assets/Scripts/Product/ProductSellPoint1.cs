using UnityEngine;
using System.Collections;

public class ProductSellPoint1 : MonoBehaviour, ITaskGiver
{
    [SerializeField] private ProductType1 acceptedProduct = ProductType.BentPipe;
    [SerializeField] private int sellPrice = 150;
    [SerializeField] private float sellInterval = 2f;

    private bool isSelling = false;
    private int productsInStock = 0;

    public bool CanAcceptProduct(ProductType1 product) => product == acceptedProduct;

    public void ReceiveProduct(ProductType1 product, GameObject productVisual = null)
    {
        if (!CanAcceptProduct(product)) return;

        // Уничтожаем визуал (продукт поступает на склад)
        if (productVisual != null)
            Destroy(productVisual);

        productsInStock++;
        Debug.Log($"Продукт принят на продажу. На складе: {productsInStock}");

        if (!isSelling)
        {
            StartCoroutine(SellRoutine());
        }
    }

    private IEnumerator SellRoutine()
    {
        isSelling = true;

        while (productsInStock > 0)
        {
            yield return new WaitForSeconds(sellInterval);
            
            productsInStock--;
            EconomyManager1.Instance.AddMoney(sellPrice);
            Debug.Log($"Продукт продан за {sellPrice}. На складе: {productsInStock}");
        }

        isSelling = false;
    }

    // ITaskGiver implementation
    public void OnTaskCompleted(TransportTask1 task)
    {
        // Это вызывается, когда логист доставил продукт к нам
        Debug.Log($"Продукт {task.ProductType} доставлен в точку продажи");
    }

    public Vector3 GetPosition() => transform.position;
}
