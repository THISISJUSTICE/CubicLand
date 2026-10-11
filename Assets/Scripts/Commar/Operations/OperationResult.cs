namespace Commar
{
    public struct OperationResult
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }

        public OperationResult(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static OperationResult GetSuccessResult()
        {
            return new OperationResult(true, string.Empty);
        }

        public static OperationResult<T> GetSuccessResult<T>(T value)
        {
            return new OperationResult<T>(true, string.Empty, value);
        }

        public static OperationResult GetFailedResult(string errorMessage)
        {
            return new OperationResult(false, errorMessage);
        }

        public static OperationResult<T> GetFailedResult<T>(string errorMessage)
        {
            return new OperationResult<T>(false, errorMessage, default);
        }
    }

    public struct OperationResult<T>
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        public T Value { get; private set; }

        public OperationResult(bool isSuccess, string errorMessage, T value)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Value = value;
        }
    }
}