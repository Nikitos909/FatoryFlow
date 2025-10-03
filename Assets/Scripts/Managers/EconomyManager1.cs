using System;
using UnityEngine;
using TMPro;

public class EconomyManager1 : MonoBehaviour
{
    public static EconomyManager1 Instance;

    public event Action<int> OnMoneyChanged; // Событие для обновления UI

    [SerializeField] private int startingMoney = 1000;
    [SerializeField] private TextMeshProUGUI moneyText;

    private int currentMoney;

    public void Initialize()
    {
        if (Instance == null) Instance = this;

        currentMoney = startingMoney;
        Debug.Log("EconomyManager Init!");
        UpdateMoneyUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            OnMoneyChanged?.Invoke(currentMoney);
            Debug.Log("Успешная трата денег!");
            return true; // Покупка успешна
        }
        else
        {
            Debug.Log("Недостаточно денег!");
            return false; // Покупка не удалась
        }
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"Деньги: {currentMoney}";
    }

    public int GetCurrentMoney() => currentMoney;
}
