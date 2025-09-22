using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "GameConfig/MachineType")]
public class MachineTypeSO : ScriptableObject
{
    [Header("Идентификация")]
    public MachineType enumType; // Используем перечисление из Enums.cs
    public string displayName;
    public GameObject prefab;

    [Header("Производство")]
    public ProductType inputProductType;
    public ProductType outputProductType;
    public ProductType defectiveProductType;

    [Header("Характеристики")]
    public float baseProductionTime = 5f;
    [Range(0f, 1f)] public float baseDefectChance = 0.1f;
    public float breakdownChance = 0.05f;

    [Header("Экономика")]
    public int basePrice = 500;
    public int rentCostPerMinute = 10;
    public int electricityCostPerCycle = 5;

    [Header("Визуал")]
    public Sprite uiIcon;
    public Vector2 footprintSize = new Vector2(1, 1);
    public Color machineColor = Color.white;

    [Header("Апгрейды")]
    public int maxUpgradeLevel = 3;
    public float speedIncreasePerLevel = 0.2f;
    public float qualityIncreasePerLevel = 0.15f;

    // Методы для работы с конфигом
    public void ApplyConfigSettings(GameConfig config)
    {
        basePrice = config.GetMachineCost(enumType);
        baseProductionTime = config.GetBaseProductionTime(enumType);
        baseDefectChance = config.GetBaseDefectChance(enumType);
    }

    public float GetProductionTimeWithUpgrades(int upgradeLevel)
    {
        return baseProductionTime / (1f + speedIncreasePerLevel * upgradeLevel);
    }

    public float GetDefectChanceWithUpgrades(int upgradeLevel)
    {
        return Mathf.Clamp01(baseDefectChance / (1f + qualityIncreasePerLevel * upgradeLevel));
    }

    public int GetUpgradeCost(int currentLevel)
    {
        return basePrice / 2 * (currentLevel + 1);
    }

    public int CalculateRentCost()
    {
        return rentCostPerMinute + electricityCostPerCycle;
    }
}
