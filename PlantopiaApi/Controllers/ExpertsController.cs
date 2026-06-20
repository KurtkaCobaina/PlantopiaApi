using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;
using PlantopiaApi.Units;
using System.Text.RegularExpressions;

namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpertsController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;

        public ExpertsController(PlantopiaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Expert>>> GetAllExperts()
        {
            var experts = await _context.Experts.ToListAsync();
            return Ok(experts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Expert>> GetExpertById(int id)
        {
            var expert = await _context.Experts.FindAsync(id);

            if (expert == null)
            {
                return NotFound(new { message = "Эксперт не найден" });
            }

            return Ok(expert);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateExpertProfile([FromBody] UpdateExpertRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var expert = await _context.Experts.FindAsync(request.UserId);

            if (expert == null)
            {
                return NotFound(new { message = "Эксперт не найден" });
            }

            var nameRegex = @"^[а-яА-ЯёЁa-zA-Z\s]+$";
            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            var phoneRegex = @"^\+7\d{10}$";

            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                if (!Regex.IsMatch(request.FirstName.Trim(), nameRegex))
                {
                    return BadRequest(new { message = "Имя должно содержать только буквы." });
                }
                expert.FirstName = request.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                if (!Regex.IsMatch(request.LastName.Trim(), nameRegex))
                {
                    return BadRequest(new { message = "Фамилия должна содержать только буквы." });
                }
                expert.LastName = request.LastName;
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                if (!Regex.IsMatch(request.Email.Trim(), emailRegex))
                {
                    return BadRequest(new { message = "Некорректный формат email." });
                }

                var normalizedEmail = request.Email.Trim().ToLowerInvariant();
                var existingExpert = await _context.Experts.FirstOrDefaultAsync(e => e.Email == normalizedEmail && e.Id != request.UserId);
                if (existingExpert != null)
                {
                    return BadRequest(new { message = "Этот email уже занят другим экспертом." });
                }
                expert.Email = normalizedEmail;
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                if (!Regex.IsMatch(request.Phone.Trim(), phoneRegex))
                {
                    return BadRequest(new { message = "Номер телефона должен быть в формате +7XXXXXXXXXX." });
                }
                expert.Phone = request.Phone;
            }

            if (!string.IsNullOrWhiteSpace(request.Specialization))
            {
                if (!Regex.IsMatch(request.Specialization.Trim(), nameRegex))
                {
                    return BadRequest(new { message = "Специализация должна содержать только буквы." });
                }
                expert.Specialization = request.Specialization;
            }

            if (!string.IsNullOrWhiteSpace(request.Country))
            {
                if (!Regex.IsMatch(request.Country.Trim(), nameRegex))
                {
                    return BadRequest(new { message = "Страна должна содержать только буквы." });
                }
                expert.Country = request.Country;
            }

            if (!string.IsNullOrWhiteSpace(request.Region))
            {
                if (!Regex.IsMatch(request.Region.Trim(), nameRegex))
                {
                    return BadRequest(new { message = "Регион должен содержать только буквы." });
                }
                expert.Region = request.Region;
            }

            if (!string.IsNullOrWhiteSpace(request.City))
            {
                if (!Regex.IsMatch(request.City.Trim(), nameRegex))
                {
                    return BadRequest(new { message = "Город должен содержать только буквы." });
                }
                expert.City = request.City;
            }

            if (request.ExperienceYears.HasValue)
            {
                if (request.ExperienceYears.Value <= 0 || request.ExperienceYears.Value >= 100)
                {
                    return BadRequest(new { message = "Стаж работы должен быть больше 0 и меньше 100 лет." });
                }
                expert.ExperienceYears = request.ExperienceYears.Value;
            }

            if (request.HourlyRate.HasValue)
            {
                if (request.HourlyRate.Value <= 500 || request.HourlyRate.Value >= 100000)
                {
                    return BadRequest(new { message = "Часовая ставка должна быть больше 500 и меньше 100000." });
                }
                expert.HourlyRate = request.HourlyRate.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpertExists(request.UserId))
                {
                    return NotFound(new { message = "Эксперт не найден при сохранении" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "Профиль эксперта успешно обновлен" });
        }

        private bool ExpertExists(int id)
        {
            return _context.Experts.Any(e => e.Id == id);
        }
    }
}