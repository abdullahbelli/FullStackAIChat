using System.IO;
using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddControllers();

// EF Core (SQLite)
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(connStr));

// Options & Services
builder.Services.Configure<SentimentOptions>(builder.Configuration.GetSection("Sentiment"));
builder.Services.AddHttpClient<ISentimentClient, SentimentClient>();
builder.Services.AddScoped<IMessageService, MessageService>();

// CORS: origin'i env var'dan oku (Vercel/Render için pratik)
const string CorsPolicy = "_frontend";
var clientOrigin = builder.Configuration["ClientOrigin"] ?? "http://localhost:5173";

builder.Services.AddCors(o =>
    o.AddPolicy(CorsPolicy, p => p
        .WithOrigins(clientOrigin)
        .AllowAnyHeader()
        .AllowAnyMethod()
    )
);

// Swagger (Production'da da env ile açılabilir)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FullStack AI Chat API",
        Version = "v1"
    });
});

var app = builder.Build();

// ---------- Startup chores ----------
// /tmp klasörünü ve SQLite dosyasını garanti et (Render uyumu)
try
{
    var csb = new SqliteConnectionStringBuilder(connStr);
    var dataSourcePath = Path.GetFullPath(csb.DataSource ?? "messages.db");
    var dir = Path.GetDirectoryName(dataSourcePath);
    if (!string.IsNullOrEmpty(dir))
        Directory.CreateDirectory(dir);

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // istersen Migrate() de kullanabilirsin
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database bootstrap error.");
}

// Swagger'ı Production'da açmak için: Swagger__Enabled=true yap
var enableSwagger = app.Environment.IsDevelopment() ||
                    builder.Configuration.GetValue<bool>("Swagger:Enabled");

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FullStack AI Chat API v1");
    });
}

// HTTP kullanıyoruz; HTTPS redirect yok
// app.UseHttpsRedirection();

// CORS (MapControllers'tan önce)
app.UseCors(CorsPolicy);

// Basit sağlık testi (kök 404 olmasın)
app.MapGet("/", () => Results.Ok("FullStack AI Chat API is running."));

app.MapControllers();

app.Run();
