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
    /// Регистрация нового пользователя (Фермера)
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

    /// <summary>
    /// Регистрация нового ЭКСПЕРТА
    /// </summary>
    [HttpPost("register-expert")]
    public IActionResult RegisterExpert([FromBody] Expert expert)
    {
        // Валидация обязательных полей
        if (string.IsNullOrWhiteSpace(expert?.Email) || string.IsNullOrWhiteSpace(expert.Password))
        {
            return BadRequest(new { message = "Email и пароль обязательны." });
        }

        if (string.IsNullOrWhiteSpace(expert.Specialization))
        {
            return BadRequest(new { message = "Специализация обязательна для эксперта." });
        }

        // Нормализуем email
        var emailNormalized = expert.Email.Trim().ToLowerInvariant();

        // Проверяем уникальность Email среди экспертов
        // (Можно также проверить и среди Users, если хотите глобальную уникальность email во всей системе)
        var existingExpertEmails = _context.Experts
            .Select(e => e.Email)
            .ToList();

        if (existingExpertEmails.Any(e => 
                !string.IsNullOrEmpty(e) && 
                e.Trim().ToLowerInvariant() == emailNormalized))
        {
            return BadRequest(new { message = "Эксперт с таким email уже существует." });
        }

        // Устанавливаем значения по умолчанию
        expert.Email = emailNormalized;
        expert.CreatedAt = DateTime.UtcNow;
        expert.IsAvailable = true; // По умолчанию эксперт доступен
        
        // Если есть поле UpdatedAt в модели Expert, раскомментируйте:
        // expert.UpdatedAt = DateTime.UtcNow;

        // Обнуляем навигационные свойства, если они есть в модели Expert
        // expert.ConsultationsAsExpert = null; 

        // Сохраняем в БД
        _context.Experts.Add(expert);
        _context.SaveChanges();

        // Возвращаем подтверждение (без пароля)
        return Created($"/api/experts/{expert.Id}", new
        {
            id = expert.Id,
            email = expert.Email,
            firstName = expert.FirstName,
            lastName = expert.LastName,
            phone = expert.Phone,
            specialization = expert.Specialization,
            experienceYears = expert.ExperienceYears,
            hourlyRate = expert.HourlyRate,
            country = expert.Country,
            region = expert.Region,
            city = expert.City
        });
    }
}