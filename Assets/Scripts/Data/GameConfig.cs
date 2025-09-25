using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "GameConfig/Setting")]
public class GameConfig : ScriptableObject
{
    [Header("Базовые настройки")]
    public int startMoney = 1000;
    public float rawPipeSpawnInterval = 8f;

    [Header("Экономика")]
    public int workerSalary = 30;
    public float salaryInterval = 20f;

    [Header("Производство")]
    public float baseProductionTime = 5f;
    public float baseDefectChance = 0.1f;
}