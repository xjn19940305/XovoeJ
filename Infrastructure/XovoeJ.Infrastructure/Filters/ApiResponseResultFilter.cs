using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using XovoeJ.Contracts.Common;

namespace XovoeJ.Infrastructure.Filters
{
    public class ApiResponseResultFilter : IAsyncResultFilter
    {
        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            context.HttpContext.Response.Headers["X-Trace-Id"] = context.HttpContext.TraceIdentifier;

            if (context.Result is FileResult)
            {
                return next();
            }

            if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(ApiResponse.Success<object?>(null))
                {
                    StatusCode = StatusCodes.Status200OK,
                };
                return next();
            }

            if (context.Result is ObjectResult objectResult)
            {
                if (IsApiResponse(objectResult.Value))
                {
                    return next();
                }

                var statusCode = objectResult.StatusCode ?? context.HttpContext.Response.StatusCode;
                if (statusCode == 0)
                {
                    statusCode = StatusCodes.Status200OK;
                }

                var hasDataProperty = TryGetPropertyValue(objectResult.Value, "data", out var dataValue);
                var message = TryGetStringProperty(objectResult.Value, "message");

                if (statusCode >= StatusCodes.Status400BadRequest)
                {
                    objectResult.Value = ApiResponse.Fail(
                        MapErrorCode(statusCode),
                        message ?? MapErrorMessage(statusCode),
                        hasDataProperty ? dataValue : null);
                    objectResult.StatusCode = statusCode;
                    return next();
                }

                objectResult.Value = ApiResponse.Success(
                    hasDataProperty ? dataValue : objectResult.Value,
                    message ?? "success");
                objectResult.StatusCode = statusCode;
            }

            return next();
        }

        private static bool IsApiResponse(object? value)
        {
            if (value == null)
            {
                return false;
            }

            var type = value.GetType();
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
        }

        private static bool TryGetPropertyValue(object? source, string propertyName, out object? value)
        {
            value = null;
            if (source == null)
            {
                return false;
            }

            var property = GetProperty(source.GetType(), propertyName);
            if (property == null)
            {
                return false;
            }

            value = property.GetValue(source);
            return true;
        }

        private static string? TryGetStringProperty(object? source, string propertyName)
        {
            if (!TryGetPropertyValue(source, propertyName, out var value))
            {
                return null;
            }

            return value?.ToString();
        }

        private static PropertyInfo? GetProperty(Type type, string propertyName)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(property => string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase));
        }

        private static int MapErrorCode(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => 400000,
                StatusCodes.Status401Unauthorized => 401000,
                StatusCodes.Status403Forbidden => 403000,
                StatusCodes.Status404NotFound => 404000,
                _ => 500000,
            };
        }

        private static string MapErrorMessage(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "bad request",
                StatusCodes.Status401Unauthorized => "unauthorized",
                StatusCodes.Status403Forbidden => "forbidden",
                StatusCodes.Status404NotFound => "not found",
                _ => "internal server error",
            };
        }
    }
}
