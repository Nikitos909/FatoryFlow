using System;
using UnityEngine;

[System.Serializable]
public class TransportTask
{
    public Machine sourceMachine;
    public Machine destinationMachine; // null = доставка на склад
    public ProductType productType;
    public float timestamp; // время создания задачи

    public TransportTask(Machine sourceMachine, Machine dest, ProductType type)
    {
        this.sourceMachine = sourceMachine;
        destinationMachine = dest;
        productType = type;
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

