using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "GameConfig/Settings")]
public class GameConfig : ScriptableObject
{
    [Header("Настройки игры")]
    public int StartMoney = 1000;
    public float BaseProductionTime = 5f;
    public float BaseDefectChance = 0.1f;

    [Header("Стоимости")]
    public int MachineBasePrice = 500;
    public int WorkerSalary = 100;
    public int ResourceCost = 50;

    [Header("Шансы событий")]
    public float BreakdownChance = 0.05f;
    public float IllnessChance = 0.02f;

    [Header("Временные интервалы")]
    public float ExpenseInterval = 60f; // Интервал списания расходов в секундах
    public float BreakdownDuration = 30f; // Длительность поломки
    public float IllnessDuration = 45f; // Длительность болезни

    [Header("Множители апгрейдов")]
    public float SpeedUpgradeMultiplier = 1.5f;
    public float QualityUpgradeMultiplier = 2f;
    public int UpgradeCost = 300;

    public int GetMachineCost(MachineType type)
    {
        return type switch
        {
            MachineType.Cutter => MachineBasePrice,
            MachineType.Bender => MachineBasePrice * 2,
            MachineType.Welder => MachineBasePrice * 3,
            MachineType.QualityControl => MachineBasePrice * 4,
            _ => MachineBasePrice
        };
    }

    public int GetProductionCost(MachineType type)
    {
        return type switch
        {
            MachineType.Cutter => ResourceCost,
            MachineType.Bender => ResourceCost * 2,
            MachineType.Welder => ResourceCost * 3,
            MachineType.QualityControl => ResourceCost,
            _ => ResourceCost
        };
    }

    public float GetBaseProductionTime(MachineType type)
    {
        return type switch
        {
            MachineType.Cutter => BaseProductionTime,
            MachineType.Bender => BaseProductionTime * 1.5f,
            MachineType.Welder => BaseProductionTime * 2f,
            MachineType.QualityControl => BaseProductionTime * 0.5f, // Быстрая проверка
            _ => BaseProductionTime
        };
    }

    public float GetBaseDefectChance(MachineType type)
    {
        return type switch
        {
            MachineType.Cutter => BaseDefectChance,
            MachineType.Bender => BaseDefectChance * 1.2f,
            MachineType.Welder => BaseDefectChance * 1.5f,
            MachineType.QualityControl => 0f, // Контроль качества не производит брак
            _ => BaseDefectChance
        };
    }
}
