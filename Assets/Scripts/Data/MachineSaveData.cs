using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Классы для сохранения состояния объектов на сцене
[Serializable]
public class MachineSaveData
{
    public string MachineId; // Уникальный ID для связи с префабом
    public float PositionX;
    public float PositionY;
    public int Level; // Уровень апгрейда
    public bool IsBroken;
    // ... любые другие данные станка
}
