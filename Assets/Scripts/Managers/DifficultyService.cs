using UnityEngine;

public class DifficultyService : MonoBehaviour
{
    public static DifficultyService Instance;

    public void Initialize(GameConfig config) { /* ... */ }

    public bool CheckForDefect(Machine machine)
    {
        float defectChance = machine.BaseDefectChance;
        
        // Увеличиваем шанс брака если станок старый
        defectChance += machine.Age * 0.01f;
        
        return UnityEngine.Random.value < defectChance;
    }

    public bool CheckForBreakdown(Machine machine)
    {
        float breakdownChance = machine.BreakdownChance;
        // Ломается чаще если работает на максимальной скорости
        breakdownChance += machine.CurrentSpeedMultiplier * 0.1f;
        
        return UnityEngine.Random.value < breakdownChance;
    }

    public bool CheckForSickness(Logist logist)
    {
        float sicknessChance = 0.01f; // Базовый шанс
        // Усталые логисты болеют чаще
        sicknessChance += logist.Fatigue * 0.2f;
        
        return UnityEngine.Random.value < sicknessChance;
    }
}
