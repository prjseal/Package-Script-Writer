namespace PSW.Middleware;

using System.Threading.Tasks;

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Xss-Protection", "0");
        context.Response.Headers.Append("Referrer-Policy", "no-referrer");
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Remove("X-Powered-By");
            context.Response.Headers.Remove("Server");
            return Task.CompletedTask;
        });
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' www.googletagmanager.com cdnjs.cloudflare.com cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline' cdnjs.cloudflare.com cdn.jsdelivr.net; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "connect-src 'self' *.google-analytics.com *.analytics.google.com *.googletagmanager.com; " +
            "frame-ancestors 'self'");
        context.Response.Headers.Append("Permissions-Policy", "fullscreen=(), geolocation=()");
        return _next(context);
    }
}