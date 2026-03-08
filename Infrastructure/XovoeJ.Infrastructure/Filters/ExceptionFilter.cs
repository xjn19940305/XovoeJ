using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using XovoeJ.Contracts.Common;

namespace XovoeJ.Infrastructure.Filters
{
    public class ExceptionFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (!context.ExceptionHandled)
            {
                context.HttpContext.Response.Headers["X-Trace-Id"] = context.HttpContext.TraceIdentifier;
                context.Result = new ObjectResult(ApiResponse.Fail<object?>(500000, context.Exception.Message))
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                };
            }

            context.ExceptionHandled = true;
            return Task.CompletedTask;
        }
    }
}
