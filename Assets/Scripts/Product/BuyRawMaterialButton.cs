using UnityEngine;
using UnityEngine.UI;

public class BuyRawMaterialButton : MonoBehaviour
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnBuyButtonClick);
        }
    }

    private void OnBuyButtonClick()
    {
        if (GameManager.Instance != null && GameManager.Instance.warehouse != null)
        {
            GameManager.Instance.warehouse.BuyRawMaterial();
        }
        else
        {
            Debug.LogError("❌ Склад сырья не найден!");
        }
    }
}