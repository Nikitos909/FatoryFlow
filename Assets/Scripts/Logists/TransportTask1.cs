using UnityEngine;

public class TransportTask1
{
    public ProductType1 ProductType { get; private set; }
    public Vector3 FromPosition { get; private set; }
    public Vector3 ToPosition { get; private set; }
    public ITaskGiver1 TaskGiver { get; private set; } // Тот, кто выдал задание

    public TransportTask1(ProductType1 productType, Vector3 from, Vector3 to, ITaskGiver1 giver)
    {
        ProductType = productType;
        FromPosition = from;
        ToPosition = to;
        TaskGiver = giver;
    }
}
