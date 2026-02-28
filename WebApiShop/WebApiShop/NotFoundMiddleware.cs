

namespace WebApiShop
{
public class NotFoundMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NotFoundMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public NotFoundMiddleware(RequestDelegate next, ILogger<NotFoundMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        await _next(httpContext);

        if (httpContext.Response.StatusCode == 404 && !httpContext.Response.HasStarted)
        {
            _logger.LogError("404 נצפה בכתובת: {Path}", httpContext.Request.Path);

            var filePath = Path.Combine(_env.WebRootPath, "404.html");

            if (File.Exists(filePath))
            {
                httpContext.Response.ContentType = "text/html; charset=utf-8";
                await httpContext.Response.SendFileAsync(filePath);
            }
            else
            {
                await httpContext.Response.WriteAsync("Error 404: Page not found.");
            }
        }
    }
}}