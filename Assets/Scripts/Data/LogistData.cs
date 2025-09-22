using System;
using UnityEngine;

[Serializable]
public struct LogistData
{
    public Vector2 position;
    public bool isSick;
    public float fatigue;
    public TaskData currentTask;
}
