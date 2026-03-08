using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using XovoeJ.Contracts.Common;
using XovoeJ.Infrastructure.Filters;
using Xunit;

namespace XovoeJ.Api.Tests.Api
{
    public class ApiResponseEnvelopeTests
    {
        [Fact]
        public async Task WrapsSuccessObjectIntoApiResponse()
        {
            var filter = new ApiResponseResultFilter();
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var objectResult = new OkObjectResult(new { data = new { id = 1 }, message = "ok" });
            var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), objectResult, controller: new object());

            await filter.OnResultExecutionAsync(context, () => Task.FromResult(CreateExecutedContext(actionContext, context.Result)));

            var result = Assert.IsAssignableFrom<ObjectResult>(context.Result);
            var payload = Assert.IsType<ApiResponse<object>>(result.Value);
            Assert.Equal(0, payload.Code);
            Assert.Equal("ok", payload.Message);
            Assert.NotNull(payload.Data);
            Assert.True(httpContext.Response.Headers.ContainsKey("X-Trace-Id"));
        }

        [Fact]
        public async Task WrapsFailureObjectIntoApiResponse()
        {
            var filter = new ApiResponseResultFilter();
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var objectResult = new BadRequestObjectResult(new { message = "优惠券已失效" });
            var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), objectResult, controller: new object());

            await filter.OnResultExecutionAsync(context, () => Task.FromResult(CreateExecutedContext(actionContext, context.Result)));

            var result = Assert.IsAssignableFrom<ObjectResult>(context.Result);
            var payload = Assert.IsType<ApiResponse<object>>(result.Value);
            Assert.Equal(400000, payload.Code);
            Assert.Equal("优惠券已失效", payload.Message);
            Assert.Null(payload.Data);
            Assert.True(httpContext.Response.Headers.ContainsKey("X-Trace-Id"));
        }

        [Fact]
        public async Task ExceptionFilterReturnsApiResponse()
        {
            var filter = new ExceptionFilter();
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = new InvalidOperationException("boom"),
            };

            await filter.OnExceptionAsync(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            var payload = Assert.IsType<ApiResponse<object>>(result.Value);
            Assert.Equal(500000, payload.Code);
            Assert.Equal("boom", payload.Message);
            Assert.True(httpContext.Response.Headers.ContainsKey("X-Trace-Id"));
        }

        private static ResultExecutedContext CreateExecutedContext(ActionContext actionContext, IActionResult result)
        {
            return new ResultExecutedContext(actionContext, new List<IFilterMetadata>(), result, controller: new object());
        }
    }
}
