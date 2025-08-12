using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace UserManagementAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse();
            var statusCode = HttpStatusCode.InternalServerError;

            // Determine the appropriate status code and message based on exception type
            switch (exception)
            {
                case ArgumentNullException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse.Error = "Invalid request data";
                    errorResponse.Message = argEx.Message;
                    break;

                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse.Error = "Invalid argument";
                    errorResponse.Message = argEx.Message;
                    break;

                case InvalidOperationException invOpEx:
                    statusCode = HttpStatusCode.Conflict;
                    errorResponse.Error = "Business rule violation";
                    errorResponse.Message = invOpEx.Message;
                    break;

                case UnauthorizedAccessException unauthEx:
                    statusCode = HttpStatusCode.Unauthorized;
                    errorResponse.Error = "Unauthorized access";
                    errorResponse.Message = unauthEx.Message;
                    break;

                case KeyNotFoundException keyEx:
                    statusCode = HttpStatusCode.NotFound;
                    errorResponse.Error = "Resource not found";
                    errorResponse.Message = keyEx.Message;
                    break;

                case TimeoutException timeoutEx:
                    statusCode = HttpStatusCode.RequestTimeout;
                    errorResponse.Error = "Request timeout";
                    errorResponse.Message = timeoutEx.Message;
                    break;

                case HttpRequestException httpEx:
                    statusCode = HttpStatusCode.BadGateway;
                    errorResponse.Error = "External service error";
                    errorResponse.Message = httpEx.Message;
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    errorResponse.Error = "Internal server error";
                    errorResponse.Message = _environment.IsDevelopment() 
                        ? exception.Message 
                        : "An unexpected error occurred. Please try again later.";
                    break;
            }

            // Set the response status code
            response.StatusCode = (int)statusCode;

            // Add additional details in development environment
            if (_environment.IsDevelopment())
            {
                errorResponse.Details = new
                {
                    ExceptionType = exception.GetType().Name,
                    StackTrace = exception.StackTrace,
                    InnerException = exception.InnerException?.Message
                };
            }

            // Add correlation ID for tracking
            errorResponse.CorrelationId = context.TraceIdentifier;
            errorResponse.Timestamp = DateTime.UtcNow;

            // Log the exception with appropriate level
            var logLevel = statusCode == HttpStatusCode.InternalServerError ? LogLevel.Error : LogLevel.Warning;
            _logger.Log(logLevel, exception, 
                "Exception occurred while processing request: {Method} {Path} - Status: {StatusCode} - Error: {Error}", 
                context.Request.Method, context.Request.Path, (int)statusCode, errorResponse.Error);

            // Serialize and send the error response
            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            });

            await response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public object? Details { get; set; }
    }

    // Extension method for easy middleware registration
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
