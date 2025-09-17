using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LogistSaveData
{
    public string LogistId;
    public float PositionX;
    public float PositionY;
    public int CapacityLevel;
    public int SpeedLevel;
    public bool IsSick;
    // ... другие данные логиста
}
