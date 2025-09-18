public class TransportTask
{
    public Product Product;
    public Machine SourceMachine;
    public Machine TargetMachine;
    public TaskStatus Status;
}

public enum TaskStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}
