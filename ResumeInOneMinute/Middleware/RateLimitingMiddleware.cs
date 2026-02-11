using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using ResumeInOneMinute.Domain.Interface;
using System.Net;
using System.Security.Claims;

namespace ResumeInOneMinute.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context, ISubscriptionService subscriptionService)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (long.TryParse(userIdClaim, out var userId))
        {
            var benefits = await subscriptionService.GetUserBenefitsAsync(userId);
            
            if (benefits != null && benefits.TryGetValue("RATE_LIMIT_PER_MINUTE", out var limitStr) && int.TryParse(limitStr, out var limit))
            {
                var cacheKey = $"rate_limit_{userId}_{DateTime.UtcNow:yyyyMMddHHmm}";
                int requestCount = _cache.Get<int?>(cacheKey) ?? 0;

                if (requestCount >= limit)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { Status = false, Message = $"Rate limit exceeded ({limit} req/min). Please upgrade your plan for higher limits." });
                    return;
                }

                _cache.Set(cacheKey, requestCount + 1, TimeSpan.FromMinutes(2));
            }
        }

        await _next(context);
    }
}
