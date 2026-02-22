using Services;
using Entities;

namespace WebApiShop
{
public class RatingMiddleware
{
    private readonly RequestDelegate _next;

    public RatingMiddleware(RequestDelegate next)
    {
        _next = next;
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

        await ratingService.AddRating(rating);

        await _next(httpcontext);
    }
}}