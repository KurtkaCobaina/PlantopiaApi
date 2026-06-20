using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;
using PlantopiaApi.Units;

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

        [HttpPost("save")]
        public async Task<IActionResult> SaveNdviMap([FromBody] NdviMapRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Данные не предоставлены" });
                }

                if (request.UserId <= 0)
                {
                    return BadRequest(new { error = "ID пользователя обязателен" });
                }

                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    return NotFound(new { error = $"Пользователь с ID {request.UserId} не найден" });
                }

                var ndviMap = new NdviMap
                {
                    UserId = request.UserId,
                    DateTaken = request.DateTaken == default ? DateTime.UtcNow : request.DateTaken,
                    MapUrl = request.MapUrl ?? string.Empty,
                    MinNdviValue = Convert.ToDecimal(request.MinNdviValue),
                    MaxNdviValue = Convert.ToDecimal(request.MaxNdviValue),
                    AvgNdviValue = Convert.ToDecimal(request.AvgNdviValue),
                    CloudFilterApplied = request.CloudFilterApplied,
                    CreatedAt = DateTime.UtcNow
                };

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
}