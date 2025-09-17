using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "GameConfig/Settings")]
public class GameConfig : ScriptableObject
{
    public float BaseProductionTime;
    public float BaseLogistSpeed;
    public float BaseDefectChance;
    public int MachineBasePrice;
}
