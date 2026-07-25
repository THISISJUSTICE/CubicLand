namespace CustomTIJI
{
    public interface IOperationHandle
    {
        public OperationResult Result { get; }
        public bool IsCompleted { get; }
    }
}