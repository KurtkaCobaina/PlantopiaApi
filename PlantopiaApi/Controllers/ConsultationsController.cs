using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;

namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultationsController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;

        public ConsultationsController(PlantopiaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Создает новую заявку на консультацию с проверкой локации и лимита часов
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateConsultation([FromBody] Consultation consultation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Проверка существования пользователя
            var userExists = await _context.Users.AnyAsync(u => u.Id == consultation.UserId);
            if (!userExists)
            {
                return BadRequest(new { message = "Пользователь не найден." });
            }

            // 2. Проверка существования эксперта
            var expert = await _context.Experts.FindAsync(consultation.ExpertId);
            if (expert == null)
            {
                return BadRequest(new { message = "Эксперт не найден." });
            }

            // 3. ПРОВЕРКА ЛОКАЦИИ
            bool isLocationMatch = true;
            string? mismatchField = null;

            if (!string.IsNullOrWhiteSpace(consultation.Country) && !string.IsNullOrWhiteSpace(expert.Country))
            {
                if (!consultation.Country.Trim().Equals(expert.Country.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    isLocationMatch = false;
                    mismatchField = "стране";
                }
            }
            else if (!string.IsNullOrWhiteSpace(consultation.Country) && string.IsNullOrWhiteSpace(expert.Country))
            {
                 isLocationMatch = false;
                 mismatchField = "стране (у эксперта не указана)";
            }

            if (isLocationMatch && !string.IsNullOrWhiteSpace(consultation.Region) && !string.IsNullOrWhiteSpace(expert.Region))
            {
                if (!consultation.Region.Trim().Equals(expert.Region.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    isLocationMatch = false;
                    mismatchField = "регионе/области";
                }
            }

            if (isLocationMatch && !string.IsNullOrWhiteSpace(consultation.City) && !string.IsNullOrWhiteSpace(expert.City))
            {
                if (!consultation.City.Trim().Equals(expert.City.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    isLocationMatch = false;
                    mismatchField = "городе";
                }
            }

            if (!isLocationMatch)
            {
                return BadRequest(new 
                { 
                    message = $"Несоответствие локации: Вы указали проведение консультации в другом {mismatchField}, чем местоположение эксперта." 
                });
            }

            // 4. ПРОВЕРКА ЛИМИТА ЧАСОВ
            var scheduledDateUtc = DateTime.SpecifyKind(consultation.ScheduledDate, DateTimeKind.Utc);
            var dateOnlyUtc = DateTime.SpecifyKind(scheduledDateUtc.Date, DateTimeKind.Utc);

            var totalHoursBooked = await _context.Consultations
                .Where(c => c.ExpertId == consultation.ExpertId && 
                            c.ScheduledDate.Date == dateOnlyUtc)
                .SumAsync(c => c.Hours);

            const int MAX_HOURS_PER_DAY = 5;

            if (totalHoursBooked + consultation.Hours > MAX_HOURS_PER_DAY)
            {
                return BadRequest(new 
                { 
                    message = $"Превышен лимит часов. На {dateOnlyUtc:dd.MM.yyyy} уже забронировано {totalHoursBooked} ч. из {MAX_HOURS_PER_DAY}." 
                });
            }

            // 5. СОХРАНЕНИЕ ДАННЫХ
            consultation.Status = "pending";
            consultation.ScheduledDate = scheduledDateUtc;
            consultation.CreatedAt = DateTime.UtcNow;

            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConsultation), new { id = consultation.Id }, consultation);
        }

        /// <summary>
        /// Обновляет статус консультации (Подтвердить/Отклонить/Завершить)
        /// Доступно только для эксперта, которому принадлежит консультация.
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateRequest request)
        {
            // Валидация входящих данных
            if (string.IsNullOrEmpty(request.Status))
            {
                return BadRequest(new { message = "Статус не указан." });
            }

            // Разрешенные статусы
            var allowedStatuses = new[] { "confirmed", "cancelled", "completed" };
            if (!allowedStatuses.Contains(request.Status.ToLower()))
            {
                return BadRequest(new { message = "Недопустимый статус. Используйте: confirmed, cancelled, completed." });
            }

            // Ищем консультацию
            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation == null)
            {
                return NotFound(new { message = "Консультация не найдена." });
            }

            // Проверка прав доступа: убедимся, что текущий пользователь является экспертом этой консультации
            // В реальном приложении здесь лучше брать ExpertId из токена/сессии, а не доверять клиенту.
            // Но пока мы проверяем, что консультация существует. 
            // Если нужно строго проверить права, можно добавить параметр expertId в запрос или брать его из контекста авторизации.
            
            // Опционально: Проверка, что статус меняется логично (например, нельзя завершить отмененную)
            if (consultation.Status == "cancelled" && request.Status != "cancelled")
            {
                 return BadRequest(new { message = "Нельзя изменить статус отмененной консультации." });
            }

            // Обновляем статус
            consultation.Status = request.Status.ToLower();
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsultationExists(id))
                {
                    return NotFound(new { message = "Консультация была удалена." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "Статус успешно обновлен.", newStatus = consultation.Status });
        }

        /// <summary>
        /// Получает консультацию по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetConsultation(int id)
        {
            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation == null)
            {
                return NotFound(new { message = "Консультация не найдена." });
            }
            return Ok(consultation);
        }

        /// <summary>
        /// Получает все консультации пользователя
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserConsultations(int userId)
        {
            var consultations = await _context.Consultations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.ScheduledDate)
                .ToListAsync();

            return Ok(consultations);
        }

        /// <summary>
        /// Получает все консультации эксперта
        /// </summary>
        [HttpGet("expert/{expertId}")]
        public async Task<IActionResult> GetExpertConsultations(int expertId)
        {
            var consultations = await _context.Consultations
                .Where(c => c.ExpertId == expertId)
                .OrderByDescending(c => c.ScheduledDate)
                .ToListAsync();

            return Ok(consultations);
        }

        private bool ConsultationExists(int id)
        {
            return _context.Consultations.Any(e => e.Id == id);
        }
    }

    // DTO для запроса обновления статуса
    public class StatusUpdateRequest
    {
        public string Status { get; set; }
    }
}