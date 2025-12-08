using PSW.Middleware;
using PSW.Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options => options.ViewLocationFormats.Add("/{0}.cshtml"));
builder.Services.AddHttpClient();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Package Script Writer API",
        Version = "v1",
        Description = "API for generating package installation scripts and managing package versions"
    });

    // Include XML comments in Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});
builder.Services.AddScoped<IScriptGeneratorService, ScriptGeneratorService>();
builder.Services.AddScoped<IPackageService, MarketplacePackageService>();
builder.Services.AddScoped<IQueryStringService, QueryStringService>();
builder.Services.AddScoped<IUmbracoVersionService, UmbracoVersionService>();

builder.Services.Configure<PSWConfig>(
    builder.Configuration.GetSection(PSWConfig.SectionName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Package Script Writer API v1");
    options.RoutePrefix = "api/docs"; // Access Swagger UI at /api/docs
});

app.UseHttpsRedirection();

app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Make the implicit Program class public so integration tests can access it
public partial class Program { }