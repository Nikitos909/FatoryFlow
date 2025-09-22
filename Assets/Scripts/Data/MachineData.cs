using UnityEngine;
using System;

[Serializable]
public struct MachineData
{
    public string machineTypeId;
    public Vector3 position;
    public Quaternion rotation;
    public float age;
    public int upgradeLevel;
    public bool isBroken;
    public float repairProgress;
}
