using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;

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

        /// <summary>
        /// Получает список всех экспертов
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Expert>>> GetAllExperts()
        {
            var experts = await _context.Experts.ToListAsync();
            return Ok(experts);
        }

        /// <summary>
        /// Получает эксперта по ID
        /// </summary>
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

        /// <summary>
        /// Обновляет профиль эксперта
        /// Фронтенд отправляет userId (который для эксперта равен его Id в таблице Experts) и новые данные.
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateExpertProfile([FromBody] UpdateExpertRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ищем эксперта по ID (который приходит как userId с фронтенда)
            var expert = await _context.Experts.FindAsync(request.UserId);

            if (expert == null)
            {
                return NotFound(new { message = "Эксперт не найден" });
            }

            // Обновляем поля
            expert.FirstName = request.FirstName ?? expert.FirstName;
            expert.LastName = request.LastName ?? expert.LastName;
            expert.Email = request.Email ?? expert.Email;
            expert.Phone = request.Phone ?? expert.Phone;
            
            // Специфичные поля эксперта
            expert.Specialization = request.Specialization ?? expert.Specialization;
            expert.ExperienceYears = request.ExperienceYears ?? expert.ExperienceYears;
            expert.HourlyRate = request.HourlyRate ?? expert.HourlyRate;
            expert.Country = request.Country ?? expert.Country;
            expert.Region = request.Region ?? expert.Region;
            expert.City = request.City ?? expert.City;

      

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

    // DTO для запроса обновления профиля эксперта
    public class UpdateExpertRequest
    {
        public int UserId { get; set; } // Это ID эксперта
        
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        
        public string? Specialization { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string? City { get; set; }
    }
}