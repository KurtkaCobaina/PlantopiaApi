using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

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

            if (userIdElement.ValueKind != JsonValueKind.Number || 
                !userIdElement.TryGetInt32(out var userId))
            {
                return BadRequest("'userId' должен быть целым числом.");
            }

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

            if (userIdElement.ValueKind != JsonValueKind.Number || 
                !userIdElement.TryGetInt32(out var userId))
            {
                return BadRequest("'userId' должен быть целым числом.");
            }

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
            if (!body.TryGetProperty("userId", out var userIdElement) ||
                userIdElement.ValueKind != JsonValueKind.Number ||
                !userIdElement.TryGetInt32(out var userId))
            {
                return BadRequest("Требуется поле 'userId' (целое число).");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Пользователь не найден.");

            var nameRegex = @"^[а-яА-ЯёЁa-zA-Z\s]+$";
            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            var phoneRegex = @"^\+7\d{10}$";

            if (body.TryGetProperty("first_name", out var firstNameEl) && firstNameEl.ValueKind == JsonValueKind.String)
            {
                var firstName = firstNameEl.GetString();
                if (!string.IsNullOrWhiteSpace(firstName))
                {
                    if (!Regex.IsMatch(firstName.Trim(), nameRegex))
                    {
                        return BadRequest(new { message = "Имя должно содержать только буквы." });
                    }
                    user.FirstName = firstName;
                }
            }

            if (body.TryGetProperty("last_name", out var lastNameEl) && lastNameEl.ValueKind == JsonValueKind.String)
            {
                var lastName = lastNameEl.GetString();
                if (!string.IsNullOrWhiteSpace(lastName))
                {
                    if (!Regex.IsMatch(lastName.Trim(), nameRegex))
                    {
                        return BadRequest(new { message = "Фамилия должна содержать только буквы." });
                    }
                    user.LastName = lastName;
                }
            }

            if (body.TryGetProperty("email", out var emailEl) && emailEl.ValueKind == JsonValueKind.String)
            {
                var email = emailEl.GetString();
                if (!string.IsNullOrWhiteSpace(email))
                {
                    if (!Regex.IsMatch(email.Trim(), emailRegex))
                    {
                        return BadRequest(new { message = "Некорректный формат email." });
                    }
                    
                    var normalizedEmail = email.Trim().ToLowerInvariant();
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.Id != userId);
                    if (existingUser != null)
                    {
                        return BadRequest(new { message = "Этот email уже занят другим пользователем." });
                    }
                    
                    user.Email = normalizedEmail;
                }
            }

            if (body.TryGetProperty("phone", out var phoneEl) && phoneEl.ValueKind == JsonValueKind.String)
            {
                var phone = phoneEl.GetString();
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    if (!Regex.IsMatch(phone.Trim(), phoneRegex))
                    {
                        return BadRequest(new { message = "Номер телефона должен быть в формате +7XXXXXXXXXX." });
                    }
                    user.Phone = phone;
                }
            }

            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Профиль успешно обновлён." });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Ошибка сохранения: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPut("update-subscription-status/{userId}")]
        public async Task<IActionResult> UpdateSubscriptionStatus(int userId, [FromBody] JsonElement body)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Пользователь не найден.");

            bool newStatus = true;

            if (body.TryGetProperty("active", out var activeEl))
            {
                if (activeEl.ValueKind == JsonValueKind.True)
                {
                    newStatus = true;
                }
                else if (activeEl.ValueKind == JsonValueKind.False)
                {
                    newStatus = false;
                }
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