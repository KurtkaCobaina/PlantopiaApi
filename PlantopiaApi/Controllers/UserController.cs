using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;
using System.Text.Json;

namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;

        public UserController(PlantopiaDbContext context)
        {
            _context = context;
        }

        [HttpPut("update-api-key")]
        public async Task<IActionResult> UpdateApiKey([FromBody] JsonElement body)
        {
            if (!body.TryGetProperty("userId", out var userIdElement) ||
                !body.TryGetProperty("apiKey", out var apiKeyElement))
            {
                return BadRequest("Требуются поля 'userId' и 'apiKey'.");
            }

            // Парсим userId как число
            if (userIdElement.ValueKind != JsonValueKind.Number || 
                !userIdElement.TryGetInt32(out var userId))
            {
                return BadRequest("'userId' должен быть целым числом.");
            }

            // Парсим apiKey как строку
            if (apiKeyElement.ValueKind != JsonValueKind.String)
            {
                return BadRequest("'apiKey' должен быть строкой.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Пользователь не найден.");

            user.ApiKey = apiKeyElement.GetString() ?? "";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "API ключ обновлён." });
        }
        [HttpPut("update-ndvi-api-key")]
        public async Task<IActionResult> UpdateNdviApiKey([FromBody] JsonElement body)
        {
            if (!body.TryGetProperty("userId", out var userIdElement) ||
                !body.TryGetProperty("ndviApiKey", out var ndviApiKeyElement))
            {
                return BadRequest("Требуются поля 'userId' и 'ndviApiKey'.");
            }

            // Парсим userId как число
            if (userIdElement.ValueKind != JsonValueKind.Number || 
                !userIdElement.TryGetInt32(out var userId))
            {
                return BadRequest("'userId' должен быть целым числом.");
            }

            // Парсим ndviApiKey как строку
            if (ndviApiKeyElement.ValueKind != JsonValueKind.String)
            {
                return BadRequest("'ndviApiKey' должен быть строкой.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Пользователь не найден.");

            user.NDVIApiKey = ndviApiKeyElement.GetString() ?? "";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "NDVI API ключ обновлён." });
        }
        // GET: /api/user/profile — для получения профиля (уже используется во фронте)
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile([FromQuery] int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Пользователь не найден.");

            return Ok(user);
        }
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] JsonElement body)
        {
            // Проверяем наличие userId
            if (!body.TryGetProperty("userId", out var userIdElement) ||
                userIdElement.ValueKind != JsonValueKind.Number ||
                !userIdElement.TryGetInt32(out var userId))
            {
                return BadRequest("Требуется поле 'userId' (целое число).");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Пользователь не найден.");

            // Обновляем поля, если они присутствуют
            if (body.TryGetProperty("first_name", out var firstNameEl) && firstNameEl.ValueKind == JsonValueKind.String)
                user.FirstName = firstNameEl.GetString();

            if (body.TryGetProperty("last_name", out var lastNameEl) && lastNameEl.ValueKind == JsonValueKind.String)
                user.LastName = lastNameEl.GetString();

            if (body.TryGetProperty("email", out var emailEl) && emailEl.ValueKind == JsonValueKind.String)
                user.Email = emailEl.GetString();

            if (body.TryGetProperty("phone", out var phoneEl) && phoneEl.ValueKind == JsonValueKind.String)
                user.Phone = phoneEl.GetString();

            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Профиль успешно обновлён." });
            }
            catch (DbUpdateException ex)
            {
                // Например, дубликат email
                return BadRequest($"Ошибка сохранения: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
        // Добавьте в конец класса UserController

        /// <summary>
        /// Самая простая реализация изменения статуса подписки на true
        /// (использует существующее поле, например ApiKey или другое)
        /// </summary>
        [HttpPut("update-subscription-status/{userId}")]
        public async Task<IActionResult> UpdateSubscriptionStatus(int userId, [FromBody] JsonElement body)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Пользователь не найден.");

            // По умолчанию — активировать
            bool newStatus = true;

            // Если передано поле "active", используем его
            if (body.TryGetProperty("active", out var activeEl) && 
                activeEl.ValueKind == JsonValueKind.True)
            {
                newStatus = true;
            }
            else if (activeEl.ValueKind == JsonValueKind.False)
            {
                newStatus = false;
            }

            user.SubscriptionStatus = newStatus;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                success = true,
                message = newStatus 
                    ? "Подписка успешно активирована" 
                    : "Подписка успешно отменена",
                subscriptionStatus = user.SubscriptionStatus,
                updatedAt = user.UpdatedAt
            });
        }
    }
}