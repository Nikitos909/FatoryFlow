using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    public int CurrentMoney { get; private set; } = 100; // Ñòàðòîâûé êàïèòàë

    // Ñîáûòèå äëÿ îáíîâëåíèÿ UI
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

    public void Initialize(GameConfig config)
    {
        CurrentMoney = config.StartMoney;
        ExpenseTimer = config.ExpenseInterval;
        CalculateDailyExpenses();
    }

    public void OnUpdate(float deltaTime)
    {
        // Списание периодических расходов
        ExpenseTimer -= deltaTime;
        if (ExpenseTimer <= 0)
        {
            SpendMoney(DailyExpenses);
            ExpenseTimer = Config.ExpenseInterval;
        }
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        UIManager.Instance.UpdateMoneyUI(CurrentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (CurrentMoney >= amount)
        {
            CurrentMoney -= amount;
            UIManager.Instance.UpdateMoneyUI(CurrentMoney);
            return true;
        }
        return false;
    }

    private void CalculateDailyExpenses()
    {
        int expenses = 0;
        
        // Зарплаты логистов
        foreach (var logist in LogisticsManager.Instance.AllLogists)
            expenses += logist.Salary;
        
        // Аренда станков
        foreach (var machine in ProductionManager.Instance.AllMachines)
            expenses += machine.RentCost;
        
        DailyExpenses = expenses;
    }

    public void OnMachineBought(Machine machine)
    {
        SpendMoney(machine.Price);
        CalculateDailyExpenses(); // Пересчитываем расходы
    }
}
