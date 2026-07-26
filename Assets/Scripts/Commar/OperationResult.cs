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

        public static OperationResult GetSuccessedResult() => new OperationResult(true, null);
        public static OperationResult GetFailedResult(string errorMessage) => new OperationResult(false, errorMessage);
    }
}