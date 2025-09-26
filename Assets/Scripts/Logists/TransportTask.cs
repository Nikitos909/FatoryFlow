using System;
using UnityEngine;

[System.Serializable]
public class TransportTask
{
    public Machine sourceMachine;
    public Machine destinationMachine;
    public ProductType productType;
    public int priority; // 1 - высокий, 2 - средний, 3 - низкий
    public float timestamp; // время создания задачи
    
    public TransportTask(Machine source, Machine dest, ProductType type, int prio = 2)
    {
        sourceMachine = source;
        destinationMachine = dest;
        productType = type;
        priority = prio;
        timestamp = Time.time;
    }
}
