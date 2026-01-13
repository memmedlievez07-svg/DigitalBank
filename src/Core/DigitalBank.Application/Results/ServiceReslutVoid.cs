namespace DigitalBank.Application.Results
{
    public class ServiceResultVoid
    {
        public bool Success { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public List<string> Errors { get; private set; } = new();
        public int StatusCode { get; private set; } = 200;

        public static ServiceResultVoid Ok(string message = "", int statusCode = 200)
            => new() { Success = true, Message = message, StatusCode = statusCode };

        public static ServiceResultVoid Fail(string message, int statusCode = 400)
            => new()
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = new List<string> { message }
            };

        public static ServiceResultVoid Fail(List<string> errors, string message = "Validation failed", int statusCode = 400)
            => new()
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors ?? new List<string>()
            };
    }
}
