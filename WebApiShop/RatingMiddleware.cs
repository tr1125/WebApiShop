using Services;
using Entities;
using Microsoft.Extensions.Logging;

namespace WebApiShop
{
public class RatingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RatingMiddleware> _logger;
    public RatingMiddleware(RequestDelegate next, ILogger<RatingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpcontext, IRatingService ratingService)
    {
        
// 1)	 בפונקציה Invoke של ה-Middleware, יש למלא את הנתונים הבאים מה-Context שהתקבל.
// •	HOST- כתובת האתר בה אנו גולשים כעת
// •	METHOD- המתודה אליה נגשנו)
// •	[PATH] URL ה- אליו בוצעה הפניה
// •	REFERER- הדף ממנו התבצעה הפניה
// •	USER_AGENT- מכיל את שם הדפדפן, גירסתו, מערכת ההפעלה ושפתה
// •	RECORD_DATE- תאריך הרישום לרייטינג

        string host = httpcontext.Request.Host.Value;
        string method = httpcontext.Request.Method;
        string path = httpcontext.Request.Path;
        string referer = httpcontext.Request.Headers["Referer"].ToString();
        string userAgent = httpcontext.Request.Headers["User-Agent"].ToString();
        DateTime recordDate = DateTime.UtcNow;

        Rating rating = new Rating
        {
            Host = host,
            Method = method,
            Path = path,
            Referer = referer,
            UserAgent = userAgent,
            RecordDate = recordDate
        };

        try
        {
            await ratingService.AddRating(rating);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Rating save failed for path={Path}", path);
        }

        await _next(httpcontext);
    }
}}