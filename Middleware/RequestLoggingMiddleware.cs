using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace UserManagementAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestTime = DateTime.UtcNow;

            // Log the incoming request
            await LogRequest(context, requestTime);

            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                // Call the next middleware in the pipeline
                await _next(context);

                stopwatch.Stop();

                // Log the outgoing response
                await LogResponse(context, requestTime, stopwatch.ElapsedMilliseconds);

                // Copy the response back to the original stream
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while processing the request: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task LogRequest(HttpContext context, DateTime requestTime)
        {
            var request = context.Request;
            var requestBody = string.Empty;

            // Capture request body for POST/PUT requests
            if (request.Method == "POST" || request.Method == "PUT")
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true);
                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            var logMessage = new
            {
                Timestamp = requestTime,
                Type = "Request",
                Method = request.Method,
                Path = request.Path,
                QueryString = request.QueryString.ToString(),
                Headers = GetFilteredHeaders(request.Headers),
                Body = requestBody,
                ClientIP = GetClientIP(context),
                UserAgent = request.Headers["User-Agent"].ToString()
            };

            _logger.LogInformation("Incoming Request: {@RequestLog}", logMessage);
        }

        private async Task LogResponse(HttpContext context, DateTime requestTime, long elapsedMs)
        {
            var response = context.Response;
            var responseBody = string.Empty;

            // Capture response body
            if (context.Response.Body is MemoryStream memoryStream)
            {
                memoryStream.Position = 0;
                using var reader = new StreamReader(memoryStream, Encoding.UTF8, true, 1024, true);
                responseBody = await reader.ReadToEndAsync();
                memoryStream.Position = 0;
            }

            var logMessage = new
            {
                Timestamp = DateTime.UtcNow,
                Type = "Response",
                StatusCode = response.StatusCode,
                ElapsedMs = elapsedMs,
                Headers = GetFilteredHeaders(response.Headers),
                Body = responseBody,
                RequestMethod = context.Request.Method,
                RequestPath = context.Request.Path
            };

            var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            _logger.Log(logLevel, "Outgoing Response: {@ResponseLog}", logMessage);
        }

        private Dictionary<string, string> GetFilteredHeaders(IHeaderDictionary headers)
        {
            var filteredHeaders = new Dictionary<string, string>();
            var sensitiveHeaders = new[] { "authorization", "cookie", "x-api-key" };

            foreach (var header in headers)
            {
                if (sensitiveHeaders.Contains(header.Key.ToLowerInvariant()))
                {
                    filteredHeaders[header.Key] = "[REDACTED]";
                }
                else
                {
                    filteredHeaders[header.Key] = header.Value.ToString();
                }
            }

            return filteredHeaders;
        }

        private string GetClientIP(HttpContext context)
        {
            // Try to get the real IP address from various headers
            var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                return forwardedHeader.Split(',')[0].Trim();
            }

            var realIPHeader = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIPHeader))
            {
                return realIPHeader;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }

    // Extension method for easy middleware registration
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
