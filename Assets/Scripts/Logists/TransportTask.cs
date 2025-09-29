uusing System;
using UnityEngine;

[System.Serializable]
public class TransportTask
{
    public Machine sourceMachine;
    public Machine destinationMachine; // null = доставка на склад
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

    // Для сравнения задач
    public override bool Equals(object obj)
    {
        if (obj is TransportTask other)
        {
            return sourceMachine == other.sourceMachine && 
                   destinationMachine == other.destinationMachine && 
                   productType == other.productType;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(sourceMachine, destinationMachine, productType);
    }
}

