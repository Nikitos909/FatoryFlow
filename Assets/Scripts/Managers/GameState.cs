using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameState
{
    // Экономика
    public int currentMoney;
    public int totalRevenue;
    public int totalExpenses;

    // Производство
    public List<MachineData> machinesData = new List<MachineData>();
    public List<ProductData> productsData = new List<ProductData>();

    // Персонал
    public List<LogistData> logistsData = new List<LogistData>();

    // Прогресс уровня
    public int currentLevel;
    public int ordersCompleted;
    public int productsManufactured;
    public int defectiveProducts;

    // Время игры
    public float playTime;
    public DateTime lastSaveTime;

    // Статистика эффективности
    public float overallEfficiency;
    public float productionEfficiency;
    public float logisticsEfficiency;

    public GameState()
    {
        lastSaveTime = DateTime.Now;
    }

    public void UpdateEfficiencyMetrics()
    {
        // Расчет общей эффективности
        if (productsManufactured > 0)
        {
            productionEfficiency = 1f - ((float)defectiveProducts / productsManufactured);
        }

        // Логистическая эффективность (упрощенная формула)
        logisticsEfficiency = Mathf.Clamp01(1f - (float)defectiveProducts / (productsManufactured + 1));

        overallEfficiency = (productionEfficiency + logisticsEfficiency) / 2f;
    }

    public void AddProduct(Product product, bool isDefective = false)
    {
        productsManufactured++;
        if (isDefective) defectiveProducts++;

        UpdateEfficiencyMetrics();
    }

    public void CompleteOrder(int reward)
    {
        ordersCompleted++;
        totalRevenue += reward;
    }
}
