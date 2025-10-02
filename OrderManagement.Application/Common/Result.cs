namespace OrderManagement.Application.Common
{
    /// <summary>
    /// Result pattern за обработка на грешки без exceptions
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }
        public string? Error { get; }
        public List<string> ValidationErrors { get; }

        private Result(bool isSuccess, T? data, string? error, List<string>? validationErrors = null)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
            ValidationErrors = validationErrors ?? new List<string>();
        }

        public static Result<T> Success(T data) => new(true, data, null);

        public static Result<T> Failure(string error) => new(false, default, error);

        public static Result<T> ValidationFailure(List<string> errors) => new(false, default, "Validation failed", errors);
    }
}
