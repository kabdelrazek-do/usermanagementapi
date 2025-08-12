using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace UserManagementAPI.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication for certain endpoints (like health checks, swagger, etc.)
            if (ShouldSkipAuthentication(context.Request.Path))
            {
                await _next(context);
                return;
            }

            try
            {
                var token = ExtractToken(context.Request);
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No authentication token provided for request: {Method} {Path}", 
                        context.Request.Method, context.Request.Path);
                    await ReturnUnauthorizedResponse(context, "No authentication token provided");
                    return;
                }

                if (!ValidateToken(token))
                {
                    _logger.LogWarning("Invalid authentication token provided for request: {Method} {Path}", 
                        context.Request.Method, context.Request.Path);
                    await ReturnUnauthorizedResponse(context, "Invalid authentication token");
                    return;
                }

                // Set user claims in the context
                var claims = ExtractClaimsFromToken(token);
                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

                _logger.LogInformation("Authentication successful for request: {Method} {Path} - User: {User}", 
                    context.Request.Method, context.Request.Path, GetUserIdFromClaims(claims));

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during authentication for request: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                await ReturnUnauthorizedResponse(context, "Authentication error occurred");
            }
        }

        private bool ShouldSkipAuthentication(PathString path)
        {
            var skipPaths = new[]
            {
                "/swagger",
                "/swagger-ui",
                "/openapi",
                "/health",
                "/favicon.ico"
            };

            return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath));
        }

        private string? ExtractToken(HttpRequest request)
        {
            // Try to get token from Authorization header
            var authHeader = request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            // Try to get token from X-API-Key header
            var apiKey = request.Headers["X-API-Key"].FirstOrDefault();
            if (!string.IsNullOrEmpty(apiKey))
            {
                return apiKey;
            }

            // Try to get token from query string (for testing purposes)
            var queryToken = request.Query["token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(queryToken))
            {
                return queryToken;
            }

            return null;
        }

        private bool ValidateToken(string token)
        {
            try
            {
                // For demonstration purposes, we'll use a simple token validation
                // In a real application, you would validate against a proper JWT or other token system
                
                // Check if token is not empty and has minimum length
                if (string.IsNullOrEmpty(token) || token.Length < 10)
                {
                    return false;
                }

                // Check if token starts with expected prefix (for demo purposes)
                var validPrefixes = new[] { "techhive_", "api_", "user_" };
                if (!validPrefixes.Any(prefix => token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                // Check if token contains required parts
                if (!token.Contains("_") || token.Split('_').Length < 2)
                {
                    return false;
                }

                // Additional validation could include:
                // - JWT signature verification
                // - Token expiration check
                // - Token issuer validation
                // - Token audience validation

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        private List<Claim> ExtractClaimsFromToken(string token)
        {
            var claims = new List<Claim>();

            try
            {
                // For demonstration purposes, we'll extract basic claims from the token
                // In a real application, you would decode JWT and extract actual claims

                // Extract user ID from token (assuming format: prefix_userid_otherparts)
                var parts = token.Split('_');
                if (parts.Length >= 2)
                {
                    var userId = parts[1];
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                    claims.Add(new Claim(ClaimTypes.Name, $"user_{userId}"));
                }

                // Add role based on token prefix
                if (token.StartsWith("techhive_", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }
                else if (token.StartsWith("api_", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "API"));
                }
                else if (token.StartsWith("user_", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "User"));
                }

                // Add standard claims
                claims.Add(new Claim("token_type", "Bearer"));
                claims.Add(new Claim("token_issued_at", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting claims from token");
                // Return minimal claims if extraction fails
                claims.Add(new Claim(ClaimTypes.NameIdentifier, "unknown"));
                claims.Add(new Claim(ClaimTypes.Role, "User"));
            }

            return claims;
        }

        private string GetUserIdFromClaims(IEnumerable<Claim> claims)
        {
            return claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        }

        private async Task ReturnUnauthorizedResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Unauthorized",
                message = message,
                correlationId = context.TraceIdentifier,
                timestamp = DateTime.UtcNow
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    // Extension method for easy middleware registration
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
