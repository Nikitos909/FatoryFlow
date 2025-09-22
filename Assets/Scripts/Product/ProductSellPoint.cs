using UnityEngine;

public class ProductSellPoint : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Product product = other.GetComponent<Product>();
        if (product != null && product.type == ProductType.FinalProduct)
        {
            EconomyManager.Instance.OnProductSold(product);
            Destroy(other.gameObject);
            Debug.Log("Продукт продан!");
        }
    }
}
