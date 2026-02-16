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
    public int productsManufactured;
    public int defectiveProducts;
    public int ordersCompleted;

    // Статистика
    public float overallEfficiency;
    public float productionEfficiency;
    public float logisticsEfficiency;
    public float playTime;

    public GameState()
    {
        currentMoney = 100;
    }

    public void UpdateEfficiencyMetrics()
    {
        if (productsManufactured > 0)
        {
            productionEfficiency = 1f - ((float)defectiveProducts / productsManufactured);
            logisticsEfficiency = Mathf.Clamp01(1f - (float)defectiveProducts / (productsManufactured + 1));
            overallEfficiency = (productionEfficiency + logisticsEfficiency) / 2f;
        }
    }

    public void AddProduct(bool isDefective = false)
    {
        productsManufactured++;
        if (isDefective) defectiveProducts++;
        UpdateEfficiencyMetrics();
    }
}
