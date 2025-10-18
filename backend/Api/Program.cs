using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using AiService;


var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// EF Core (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Sentiment (ai-service kütüphanesi)
builder.Services.AddSentimentService(builder.Configuration);

// Uygulama servisleri  
builder.Services.AddScoped<IMessageService, MessageService>();

// ===== CORS (vercel.app + localhost) =====
const string CorsPolicy = "_frontend";

builder.Services.AddCors(o =>
{
    o.AddPolicy(CorsPolicy, p =>
    {
        p.SetIsOriginAllowed(origin =>
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
            var host = uri.Host; // ör: full-stack-ai-chat.vercel.app

            if (host.EndsWith("vercel.app", StringComparison.OrdinalIgnoreCase))
                return true;

            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
        // .AllowCredentials() gerekmiyorsa kapalı kalsın.
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

// DB otomatik oluştur (Render ilk açılış için faydalı)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// app.UseHttpsRedirection(); // Render edge TLS kullanıyor

// CORS'u erken koy
app.UseCors(CorsPolicy);

// Swagger UI (Dev + Prod)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FullStack AI Chat API v1");
    c.RoutePrefix = "swagger"; // https://<domain>/swagger
});

// Sağlık testi
app.MapGet("/", () => Results.Ok(new
{
    ok = true,
    env = app.Environment.EnvironmentName,
    swagger = "/swagger"
}));

// Controller'lar (CORS zorunlu)
app.MapControllers().RequireCors(CorsPolicy);

app.Run();
