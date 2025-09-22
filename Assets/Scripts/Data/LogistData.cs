using System;
using UnityEngine;

[Serializable]
public struct LogistData
{
    public Vector3 position;
    public bool isSick;
    public float fatigue;
    public TaskData currentTask;
}
