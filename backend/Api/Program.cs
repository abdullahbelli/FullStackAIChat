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

// CORS
const string CorsPolicy = "_frontend";
var clientOrigin = builder.Configuration["ClientOrigin"];
builder.Services.AddCors(o =>
    o.AddPolicy(CorsPolicy, p =>
    {
        if (!string.IsNullOrWhiteSpace(clientOrigin))
            p.WithOrigins(clientOrigin).AllowAnyHeader().AllowAnyMethod();
        else
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    })
);

// Swagger — her zaman aktif
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FullStack AI Chat API",
        Version = "v1",
        Description = "ASP.NET Core + Hugging Face entegre API (Swagger Production açık)"
    });
});

var app = builder.Build();

// ---- Veritabanı otomatik oluşturulsun ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// HTTP — Render HTTPS’yi edge’de hallediyor
// app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

// ✅ Swagger her zaman aktif (Development + Production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FullStack AI Chat API v1");
    c.RoutePrefix = "swagger"; // https://.../swagger
});

// Basit sağlık testi
app.MapGet("/", () => Results.Ok(new
{
    ok = true,
    env = app.Environment.EnvironmentName,
    swagger = "/swagger"
}));

app.MapControllers();

app.Run();
