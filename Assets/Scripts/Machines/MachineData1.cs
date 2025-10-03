using UnityEngine;

[CreateAssetMenu(fileName = "NewMachineData", menuName = "Business Game/Machine Data")]
public class MachineData1 : ScriptableObject
{
    public string machineName;
    public ProductType1 inputProductType;
    public ProductType1 outputProductType;
    public float productionTime = 3f;
    public int productionCost = 0; // Стоимость производства (если есть)
}
