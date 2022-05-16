using PSW.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IScriptGeneratorService, ScriptGeneratorService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IQueryStringService, QueryStringService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>

{
    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Xss-Protection", "0");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");
    context.Response.Headers.Add("X-Powered-By", "");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self';script-src 'self' code.jquery.com;style-src 'self' cdn.rawgit.com cdn.jsdelivr.net;img-src 'self' our.umbraco.com;font-src 'self';connect-src 'self'");
    context.Response.Headers.Add("Permissions-Policy", "fullscreen=(), geolocation=()");
    await next();
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
