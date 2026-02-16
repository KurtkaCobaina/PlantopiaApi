using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using PlantopiaApi.Data;

// Добавляем HttpClient
var builder = WebApplication.CreateBuilder(args);

// Строка подключения
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена.");

// Проверка подключения (опционально)
try
{
    using var conn = new NpgsqlConnection(connectionString);
    conn.Open();
    Console.WriteLine("✅ Подключение к PostgreSQL успешно!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Ошибка подключения: {ex.Message}");
}

// Регистрация сервисов
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Plantopia API",
        Version = "v1",
        Description = "API для агротехнической платформы Plantopia"
    });
});

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Регистрация DbContext
builder.Services.AddDbContext<PlantopiaDbContext>(options =>
    options.UseNpgsql(connectionString));

// Регистрация HttpClient с настройками
builder.Services.AddHttpClient("ExternalApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "PlantopiaApi/1.0");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.Run();