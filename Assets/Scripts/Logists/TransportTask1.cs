public class TransportTask1
{
    public ProductType1 ProductType { get; private set; }
    public Vector3 FromPosition { get; private set; }
    public Vector3 ToPosition { get; private set; }
    public ITaskGiver TaskGiver { get; private set; } // Тот, кто выдал задание

    public TransportTask(ProductType productType, Vector3 from, Vector3 to, ITaskGiver giver)
    {
        ProductType = productType;
        FromPosition = from;
        ToPosition = to;
        TaskGiver = giver;
    }
}
