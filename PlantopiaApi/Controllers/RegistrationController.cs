// Controllers/RegistrationController.cs
using Microsoft.AspNetCore.Mvc;
using PlantopiaApi.Data;
using PlantopiaApi.Models;

namespace PlantopiaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly PlantopiaDbContext _context;

    public RegistrationController(PlantopiaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    [HttpPost("register")]
    public IActionResult Register([FromBody] User user)
    {
        // Валидация обязательных полей
        if (string.IsNullOrWhiteSpace(user?.Email) || string.IsNullOrWhiteSpace(user.Password))
        {
            return BadRequest(new { message = "Email и пароль обязательны." });
        }

        // Нормализуем email: убираем пробелы и приводим к нижнему регистру
        var emailNormalized = user.Email.Trim().ToLowerInvariant();

        // Загружаем все email'ы в память и проверяем уникальность
        var existingEmails = _context.Users
            .Select(u => u.Email)
            .ToList();

        if (existingEmails.Any(e => 
                !string.IsNullOrEmpty(e) && 
                e.Trim().ToLowerInvariant() == emailNormalized))
        {
            return BadRequest(new { message = "Пользователь с таким email уже существует." });
        }

        // Устанавливаем значения по умолчанию
        user.Email = emailNormalized;
        user.UserRole = "farmer";           // роль по умолчанию
        user.SubscriptionStatus = false;    // подписка отключена
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        // Обнуляем навигационные свойства (защита от ошибок сериализации)
       
        user.ConsultationsAsUser = null;
        user.Diagnoses = null;
        user.FertilizerCalculations = null;
        user.NdviMaps = null;
        user.SoilTests = null;
        user.UserTasks = null;

        // Поля ApiKey и NDVIApiKey остаются null (не генерируются)

        // Сохраняем в БД
        _context.Users.Add(user);
        _context.SaveChanges();

        // Возвращаем подтверждение (без пароля и ключей)
        return Created($"/api/users/{user.Id}", new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            phone = user.Phone,
            userRole = user.UserRole,
            subscriptionStatus = user.SubscriptionStatus
        });
    }
}