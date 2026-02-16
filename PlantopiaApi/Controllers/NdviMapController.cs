using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;

namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NdviMapController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;
        private readonly ILogger<NdviMapController> _logger;

        public NdviMapController(PlantopiaDbContext context, ILogger<NdviMapController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Сохраняет NDVI-данные (упрощенная версия без JWT)
        /// </summary>
        [HttpPost("save")]
        public async Task<IActionResult> SaveNdviMap([FromBody] NdviMapRequest request)
        {
            try
            {
                // Простая валидация
                if (request == null)
                {
                    return BadRequest(new { error = "Данные не предоставлены" });
                }

                if (request.UserId <= 0)
                {
                    return BadRequest(new { error = "ID пользователя обязателен" });
                }

                // Проверяем существование пользователя
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    return NotFound(new { error = $"Пользователь с ID {request.UserId} не найден" });
                }

                // Конвертируем double в decimal для сохранения в БД
                var ndviMap = new NdviMap
                {
                    UserId = request.UserId,
                    DateTaken = request.DateTaken == default ? DateTime.UtcNow : request.DateTaken,
                    MapUrl = request.MapUrl ?? string.Empty,
                    MinNdviValue = Convert.ToDecimal(request.MinNdviValue),  // Явное преобразование
                    MaxNdviValue = Convert.ToDecimal(request.MaxNdviValue),  // Явное преобразование
                    AvgNdviValue = Convert.ToDecimal(request.AvgNdviValue),  // Явное преобразование
                    CloudFilterApplied = request.CloudFilterApplied,
                    CreatedAt = DateTime.UtcNow
                };

                // Сохраняем в базу
                _context.NdviMaps.Add(ndviMap);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"NDVI сохранен для пользователя {request.UserId}");

                return Ok(new 
                { 
                    success = true,
                    id = ndviMap.Id, 
                    message = "NDVI-данные успешно сохранены"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении NDVI");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }
    }

    // Модель запроса с double (для приема от фронтенда)
    public class NdviMapRequest
    {
        public int UserId { get; set; }
        public DateTime DateTaken { get; set; }
        public string? MapUrl { get; set; }
        public double MinNdviValue { get; set; }  // Фронтенд отправляет double
        public double MaxNdviValue { get; set; }  // Фронтенд отправляет double
        public double AvgNdviValue { get; set; }  // Фронтенд отправляет double
        public bool CloudFilterApplied { get; set; }
    }
}