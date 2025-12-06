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
        context.Response.Headers.Append("X-Powered-By", "");
        //context.Response.Headers.Add("Content-Security-Policy", "default-src 'self';script-src 'self' code.jquery.com;style-src 'self' cdn.rawgit.com cdn.jsdelivr.net;img-src 'self' our.umbraco.com;font-src 'self';connect-src 'self'");
        context.Response.Headers.Append("Permissions-Policy", "fullscreen=(), geolocation=()");
        return _next(context);
    }
}