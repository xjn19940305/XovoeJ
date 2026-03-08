namespace XovoeJ.Contracts.Common
{
    public class ApiResponse<T>
    {
        public int Code { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }
    }

    public static class ApiResponse
    {
        public static ApiResponse<T> Success<T>(T? data = default, string message = "success")
        {
            return new ApiResponse<T>
            {
                Code = 0,
                Message = message,
                Data = data,
            };
        }

        public static ApiResponse<T> Fail<T>(int code, string message, T? data = default)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = data,
            };
        }
    }
}
