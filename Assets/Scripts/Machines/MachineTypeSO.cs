using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "GameConfig/MachineType")]
public class MachineTypeSO : ScriptableObject
{
    public MachineType machineType;
    public string displayName;
    public ProductType inputProductType;
    public ProductType outputProductType;
    public ProductType defectiveProductType;
    public float baseProductionTime = 5f;
    [Range(0f, 1f)] public float baseDefectChance = 0.1f;

    // Добавьте ссылки на префабы
    public GameObject outputProductPrefab;
    public GameObject inputProductPrefab;
}
