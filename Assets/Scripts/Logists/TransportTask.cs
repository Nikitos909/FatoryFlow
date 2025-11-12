using System;
using UnityEngine;

[System.Serializable]
public class TransportTask
{
    public Machine1 sourceMachine;
    public Machine1 destinationMachine; // null = доставка на склад
    public ProductType productType;
    public int priority; // 1 - высокий, 2 - средний, 3 - низкий
    public float timestamp; // время создания задачи

    public TransportTask(Machine1 sourceMachine, Machine1 dest, ProductType type, int prio = 2)
    {
        this.sourceMachine = sourceMachine;
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

