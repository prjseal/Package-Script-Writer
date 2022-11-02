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
        context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Xss-Protection", "0");
        context.Response.Headers.Add("Referrer-Policy", "no-referrer");
        context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");
        context.Response.Headers.Add("X-Powered-By", "");
        //context.Response.Headers.Add("Content-Security-Policy", "default-src 'self';script-src 'self' code.jquery.com;style-src 'self' cdn.rawgit.com cdn.jsdelivr.net;img-src 'self' our.umbraco.com;font-src 'self';connect-src 'self'");
        context.Response.Headers.Add("Permissions-Policy", "fullscreen=(), geolocation=()");
        return _next(context);
    }
}
