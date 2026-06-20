using Microsoft.AspNetCore.Mvc;
using PlantopiaApi.Data;
using PlantopiaApi.Models;
using System.Text.RegularExpressions;

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

    [HttpPost("register")]
    public IActionResult Register([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user?.Email) || string.IsNullOrWhiteSpace(user.Password))
        {
            return BadRequest(new { message = "Email и пароль обязательны." });
        }

        if (user.Password.Length < 6 || user.Password.Length > 128)
        {
            return BadRequest(new { message = "Пароль должен содержать от 6 до 128 символов." });
        }

        var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!Regex.IsMatch(user.Email.Trim(), emailRegex))
        {
            return BadRequest(new { message = "Некорректный формат email." });
        }

        var emailNormalized = user.Email.Trim().ToLowerInvariant();

        var existingEmails = _context.Users
            .Select(u => u.Email)
            .ToList();

        if (existingEmails.Any(e => 
                !string.IsNullOrEmpty(e) && 
                e.Trim().ToLowerInvariant() == emailNormalized))
        {
            return BadRequest(new { message = "Пользователь с таким email уже существует." });
        }

        if (!string.IsNullOrWhiteSpace(user.Phone))
        {
            var phoneRegex = @"^\+7\d{10}$";
            if (!Regex.IsMatch(user.Phone.Trim(), phoneRegex))
            {
                return BadRequest(new { message = "Номер телефона должен быть в формате +7XXXXXXXXXX (например, +79625577767)." });
            }
        }

        var nameRegex = @"^[а-яА-ЯёЁa-zA-Z\s]+$";
        
        if (!string.IsNullOrWhiteSpace(user.FirstName))
        {
            if (!Regex.IsMatch(user.FirstName.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Имя должно содержать только буквы." });
            }
        }

        if (!string.IsNullOrWhiteSpace(user.LastName))
        {
            if (!Regex.IsMatch(user.LastName.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Фамилия должна содержать только буквы." });
            }
        }

        user.Email = emailNormalized;
        user.UserRole = "farmer";
        user.SubscriptionStatus = false;
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        user.ConsultationsAsUser = null;
        user.Diagnoses = null;
        user.FertilizerCalculations = null;
        user.NdviMaps = null;
        user.SoilTests = null;
        user.UserTasks = null;

        _context.Users.Add(user);
        _context.SaveChanges();

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

    [HttpPost("register-expert")]
    public IActionResult RegisterExpert([FromBody] Expert expert)
    {
        if (string.IsNullOrWhiteSpace(expert?.Email) || string.IsNullOrWhiteSpace(expert.Password))
        {
            return BadRequest(new { message = "Email и пароль обязательны." });
        }

        if (expert.Password.Length < 6 || expert.Password.Length > 128)
        {
            return BadRequest(new { message = "Пароль должен содержать от 6 до 128 символов." });
        }

        if (string.IsNullOrWhiteSpace(expert.Specialization))
        {
            return BadRequest(new { message = "Специализация обязательна для эксперта." });
        }

        var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!Regex.IsMatch(expert.Email.Trim(), emailRegex))
        {
            return BadRequest(new { message = "Некорректный формат email." });
        }

        var emailNormalized = expert.Email.Trim().ToLowerInvariant();

        var existingExpertEmails = _context.Experts
            .Select(e => e.Email)
            .ToList();

        if (existingExpertEmails.Any(e => 
                !string.IsNullOrEmpty(e) && 
                e.Trim().ToLowerInvariant() == emailNormalized))
        {
            return BadRequest(new { message = "Эксперт с таким email уже существует." });
        }

        if (!string.IsNullOrWhiteSpace(expert.Phone))
        {
            var phoneRegex = @"^\+7\d{10}$";
            if (!Regex.IsMatch(expert.Phone.Trim(), phoneRegex))
            {
                return BadRequest(new { message = "Номер телефона должен быть в формате +7XXXXXXXXXX (например, +79625577767)." });
            }
        }

        var nameRegex = @"^[а-яА-ЯёЁa-zA-Z\s]+$";
        
        if (!string.IsNullOrWhiteSpace(expert.FirstName))
        {
            if (!Regex.IsMatch(expert.FirstName.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Имя должно содержать только буквы." });
            }
        }

        if (!string.IsNullOrWhiteSpace(expert.LastName))
        {
            if (!Regex.IsMatch(expert.LastName.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Фамилия должна содержать только буквы." });
            }
        }

        if (!string.IsNullOrWhiteSpace(expert.Specialization))
        {
            if (!Regex.IsMatch(expert.Specialization.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Специализация должна содержать только буквы." });
            }
        }

        if (!string.IsNullOrWhiteSpace(expert.Country))
        {
            if (!Regex.IsMatch(expert.Country.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Страна должна содержать только буквы." });
            }
        }

        if (!string.IsNullOrWhiteSpace(expert.Region))
        {
            if (!Regex.IsMatch(expert.Region.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Регион должен содержать только буквы." });
            }
        }

        if (!string.IsNullOrWhiteSpace(expert.City))
        {
            if (!Regex.IsMatch(expert.City.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Город должен содержать только буквы." });
            }
        }

        if (expert.ExperienceYears <= 0 || expert.ExperienceYears >= 100)
        {
            return BadRequest(new { message = "Стаж работы должен быть больше 0 и меньше 100 лет." });
        }

        if (expert.HourlyRate <= 500 || expert.HourlyRate >= 100000)
        {
            return BadRequest(new { message = "Часовая ставка должна быть больше 500 и меньше 100000." });
        }

        expert.Email = emailNormalized;
        expert.CreatedAt = DateTime.UtcNow;
        expert.IsAvailable = true;
        
        _context.Experts.Add(expert);
        _context.SaveChanges();

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