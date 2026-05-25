using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

public class IdempotencyFilter : IAsyncActionFilter
{
    private static readonly ConcurrentDictionary<string, CachedResponse> _cache = new();

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var key) ||
            string.IsNullOrWhiteSpace(key))
        {
            await next();
            return;
        }

        var idempotencyKey = key.ToString();

        if (_cache.TryGetValue(idempotencyKey, out var cached))
        {
            context.Result = new ContentResult
            {
                Content = cached.Body,
                ContentType = "application/json",
                StatusCode = cached.StatusCode
            };
            context.HttpContext.Response.Headers["X-Idempotency-Replayed"] = "true";
            return;
        }

        var executedContext = await next();

        if (executedContext.Result is ObjectResult objectResult)
        {
            var body = JsonSerializer.Serialize(objectResult.Value);
            _cache.TryAdd(idempotencyKey, new CachedResponse(objectResult.StatusCode ?? 200, body));
        }
    }

    private record CachedResponse(int StatusCode, string Body);
}
