using StackExchange.Redis;

namespace WebApiShop
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConnectionMultiplexer _redis;
        private readonly int _maxRequests;
        private readonly int _windowSeconds;

        public RateLimitMiddleware(RequestDelegate next, IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _next = next;
            _redis = redis;
            _maxRequests = configuration.GetValue<int>("RateLimit:MaxRequests");
            _windowSeconds = configuration.GetValue<int>("RateLimit:WindowSeconds");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var cacheKey = $"ratelimit_{ip}";

            var db = _redis.GetDatabase();

            // מגדיל את המונה ב-1, אם לא קיים יוצר אותו עם ערך 1
            var count = await db.StringIncrementAsync(cacheKey);

            // אם זו הבקשה הראשונה — מגדיר TTL
            if (count == 1)
            {
                await db.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(_windowSeconds));
            }

            if (count > _maxRequests)
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Too Many Requests");
                return;
            }

            await _next(context);
        }
    }
}