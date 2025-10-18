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
builder.Services.AddHttpClient<ISentimentClient, SentimentClient >();
builder.Services.AddScoped<IMessageService, MessageService>();

// CORS (frontend -> http://localhost:5173)
const string CorsPolicy = "_frontend";
builder.Services.AddCors(o =>
    o.AddPolicy(CorsPolicy, p => p
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
    )
);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "FullStack AI Chat API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FullStack AI Chat API v1");
    });
}

// HTTP kullanıyoruz; https redirect kapalı
// app.UseHttpsRedirection();

// CORS mutlaka MapControllers'tan ÖNCE
app.UseCors(CorsPolicy);

app.MapControllers();

app.Run();
