using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using AiService;

var builder = WebApplication.CreateBuilder(args);

// MVC controllerlarını ekle
builder.Services.AddControllers();

// EF Core (SQLite) bağlamı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Duygu analizi servisi 
builder.Services.AddSentimentService(builder.Configuration);

// Uygulama servisleri 
builder.Services.AddScoped<IMessageService, MessageService>();

// CORS politikası vercel.app + localhost
const string CorsPolicy = "_frontend";

builder.Services.AddCors(o =>
{
    o.AddPolicy(CorsPolicy, p =>
    {
        p.SetIsOriginAllowed(origin =>
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
            var host = uri.Host;

            if (host.EndsWith("vercel.app", StringComparison.OrdinalIgnoreCase))
                return true;

            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod();

    });
});

// Swagger 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FullStack AI Chat API",
        Version = "v1",
        Description = "ASP.NET Core + Hugging Face entegrasyonlu API (Swagger prod’da açık)"
    });
});

var app = builder.Build();

// İlk kurulumda DB'yi oluştur
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Özel CORS yanıt başlıkları 
app.Use(async (ctx, next) =>
{
    var reqOrigin = ctx.Request.Headers.Origin.ToString();
    bool isAllowed = false;

    if (!string.IsNullOrWhiteSpace(reqOrigin) &&
        Uri.TryCreate(reqOrigin, UriKind.Absolute, out var u))
    {
        var host = u.Host;
        if (host.EndsWith("vercel.app", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            isAllowed = true;
        }
    }

    if (isAllowed)
    {
        ctx.Response.Headers["Access-Control-Allow-Origin"] = reqOrigin;
        ctx.Response.Headers["Vary"] = "Origin";

        if (HttpMethods.IsOptions(ctx.Request.Method))
        {
            ctx.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,PATCH,DELETE,OPTIONS";
            ctx.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
            ctx.Response.StatusCode = StatusCodes.Status204NoContent;
            return;
        }
    }

    await next();
});

// Tanımlı CORS politikasını uygula
app.UseCors(CorsPolicy);

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FullStack AI Chat API v1");
    c.RoutePrefix = "swagger";
});

// Durum bilgisi
app.MapGet("/", () => Results.Ok(new
{
    ok = true,
    env = app.Environment.EnvironmentName,
    swagger = "/swagger"
}));

// OPTIONS istekleri için genel cevap
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok())
   .RequireCors(CorsPolicy);

// Controller route'ları
app.MapControllers().RequireCors(CorsPolicy);

app.Run();
