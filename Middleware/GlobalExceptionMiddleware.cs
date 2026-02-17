using FieldMetadataAPI.DTOs;
using System.Net;
using System.Text.Json;

namespace FieldMetadataAPI.Middleware
{
    /// <summary>
    /// Global exception handling middleware
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var apiResponse = new ApiResponse<object>();

            switch (exception)
            {
                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    apiResponse = ApiResponse<object>.ErrorResponse(
                        exception.Message,
                        new List<string> { exception.Message }
                    );
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    apiResponse = ApiResponse<object>.ErrorResponse(
                        "Resource not found",
                        new List<string> { exception.Message }
                    );
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    apiResponse = ApiResponse<object>.ErrorResponse(
                        "Unauthorized access",
                        new List<string> { exception.Message }
                    );
                    break;

                case ArgumentNullException:
                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    apiResponse = ApiResponse<object>.ErrorResponse(
                        "Invalid argument",
                        new List<string> { exception.Message }
                    );
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    apiResponse = ApiResponse<object>.ErrorResponse(
                        "An internal server error occurred",
                        new List<string> { exception.Message }
                    );
                    break;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var result = JsonSerializer.Serialize(apiResponse, options);
            await response.WriteAsync(result);
        }
    }
}
