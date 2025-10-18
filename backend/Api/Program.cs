using Api.Data;
using Microsoft.EntityFrameworkCore;
using Api.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// EF Core (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Options & Services
builder.Services.Configure<SentimentOptions>(builder.Configuration.GetSection("Sentiment"));
builder.Services.AddHttpClient<ISentimentClient, SentimentClient>();
builder.Services.AddScoped<IMessageService, MessageService>();

// ===== CORS =====
const string CorsPolicy = "_frontend";

// Birden fazla origin desteklemek için: "https://app.vercel.app,https://preview.vercel.app"
var originsCsv = builder.Configuration["ClientOrigin"] ?? string.Empty;
var origins = originsCsv
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(o =>
{
    o.AddPolicy(CorsPolicy, p =>
    {
        if (origins.Length > 0)
        {
            p.WithOrigins(origins)
             .AllowAnyHeader()
             .AllowAnyMethod();
            // Gerekirse wildcard alt alan adları:
            // p.SetIsOriginAllowedToAllowWildcardSubdomains();
        }
        else
        {
            // Origin verilmemişse, tamamen açık bırak (opsiyonel)
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

// ===== Swagger — her zaman aktif =====
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

// DB otomatik oluştur (Render ilk açılışlar için faydalı)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// HTTP — Render TLS'i edge’de hallediyor
// app.UseHttpsRedirection();

// Preflight (OPTIONS) için CORS başlıklarını garanti et
app.Use(async (ctx, next) =>
{
    if (HttpMethods.IsOptions(ctx.Request.Method))
    {
        var reqOrigin = ctx.Request.Headers.Origin.ToString();

        if (origins.Length == 0)
        {
            ctx.Response.Headers["Access-Control-Allow-Origin"] = "*";
        }
        else if (!string.IsNullOrWhiteSpace(reqOrigin) &&
                 origins.Contains(reqOrigin, StringComparer.OrdinalIgnoreCase))
        {
            ctx.Response.Headers["Access-Control-Allow-Origin"] = reqOrigin;
            ctx.Response.Headers["Vary"] = "Origin";
        }

        ctx.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,PATCH,DELETE,OPTIONS";
        ctx.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
        ctx.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }

    await next();
});

// CORS middleware'i erken
app.UseCors(CorsPolicy);

// Swagger UI her zaman açık (Development + Production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FullStack AI Chat API v1");
    c.RoutePrefix = "swagger"; // => https://<domain>/swagger
});

// Basit sağlık testi
app.MapGet("/", () => Results.Ok(new
{
    ok = true,
    env = app.Environment.EnvironmentName,
    swagger = "/swagger"
}));

// CORS politikasını controller'lara zorunlu uygula
app.MapControllers().RequireCors(CorsPolicy);

// (Opsiyonel) Her path için OPTIONS'u CORS ile eşle
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok())
   .RequireCors(CorsPolicy);

app.Run();
