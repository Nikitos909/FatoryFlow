using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    public int CurrentMoney { get; private set; } = 100; // Стартовый капитал

    // Событие для обновления UI
    public static event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        OnMoneyChanged?.Invoke(CurrentMoney); // Оповещаем UI
        Debug.Log($"Money: {CurrentMoney} (+{amount})");
    }

    public bool SpendMoney(int amount)
    {
        if (CurrentMoney >= amount)
        {
            CurrentMoney -= amount;
            OnMoneyChanged?.Invoke(CurrentMoney);
            Debug.Log($"Money: {CurrentMoney} (-{amount})");
            return true;
        }
        Debug.Log("Not enough money!");
        return false;
    }
}
