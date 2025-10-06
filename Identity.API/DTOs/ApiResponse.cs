namespace Identity.API.DTOs
{
    /// <summary>
    /// Общ API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Error { get; set; }
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; }

        public static ApiResponse<T> SuccessResult(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> ErrorResult(string error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = error
            };
        }

        public static ApiResponse<T> ErrorResult(List<string> errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = errors
            };
        }
    }
}
