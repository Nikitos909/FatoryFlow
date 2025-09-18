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
    
    public int GetProductionCost(MachineType type)
    {
        return type switch
        {
            MachineType.Cutter => ResourceCost,
            MachineType.Bender => ResourceCost * 2,
            MachineType.Welder => ResourceCost * 3,
            _ => ResourceCost
        };
    }
}

