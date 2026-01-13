namespace DigitalBank.Application.Results
{
    public class ServiceResult<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        // əlavə: çox error qaytarmaq üçün
        public List<string> Errors { get; private set; } = new();

        // əlavə: controller status seçimi üçün
        public int StatusCode { get; private set; } = 200;

        public static ServiceResult<T> Ok(T data, string message = "", int statusCode = 200)
            => new() { Success = true, Message = message, Data = data, StatusCode = statusCode };

        public static ServiceResult<T> Ok(string message = "", int statusCode = 200)
            => new() { Success = true, Message = message, Data = default, StatusCode = statusCode };

        public static ServiceResult<T> Fail(string message, int statusCode = 400)
            => new()
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = new List<string> { message }
            };

        public static ServiceResult<T> Fail(List<string> errors, string message = "Validation failed", int statusCode = 400)
            => new()
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors ?? new List<string>()
            };
    }
}
