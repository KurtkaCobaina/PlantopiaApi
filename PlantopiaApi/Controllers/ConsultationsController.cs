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
    public class ConsultationsController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;

        public ConsultationsController(PlantopiaDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateConsultation([FromBody] Consultation consultation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == consultation.UserId);
            if (!userExists)
            {
                return BadRequest(new { message = "Пользователь не найден." });
            }

            var expert = await _context.Experts.FindAsync(consultation.ExpertId);
            if (expert == null)
            {
                return BadRequest(new { message = "Эксперт не найден." });
            }

            var nameRegex = @"^[а-яА-ЯёЁa-zA-Z\s]+$";

            if (!string.IsNullOrWhiteSpace(consultation.Country))
            {
                if (!Regex.IsMatch(consultation.Country.Trim(), nameRegex))
                {
                    return BadRequest(new { message = "Страна должна содержать только буквы." });
                }
            }

            if (!string.IsNullOrWhiteSpace(consultation.Region))
            {
                if (!Regex.IsMatch(consultation.Region.Trim(), nameRegex))
                {
                    return BadRequest(new { message = "Регион должен содержать только буквы." });
                }
            }

            if (!string.IsNullOrWhiteSpace(consultation.City))
            {
                if (!Regex.IsMatch(consultation.City.Trim(), nameRegex))
                {
                    return BadRequest(new { message = "Город должен содержать только буквы." });
                }
            }

            if (consultation.Hours <= 0 || consultation.Hours >= 6)
            {
                return BadRequest(new { message = "Количество часов должно быть больше 0 и меньше 6." });
            }

            var scheduledDateUtc = DateTime.SpecifyKind(consultation.ScheduledDate, DateTimeKind.Utc);
            var nowUtc = DateTime.UtcNow;

            if (scheduledDateUtc.Date < nowUtc.Date)
            {
                return BadRequest(new { message = "Нельзя записаться на дату в прошлом." });
            }

            var maxDate = nowUtc.AddMonths(2);
            if (scheduledDateUtc.Date > maxDate.Date)
            {
                return BadRequest(new { message = "Запись возможна максимум на 2 месяца вперед." });
            }

            bool isLocationMatch = true;
            string? mismatchField = null;

            if (!string.IsNullOrWhiteSpace(consultation.Country) && !string.IsNullOrWhiteSpace(expert.Country))
            {
                if (!IsLocationSimilar(consultation.Country, expert.Country))
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
                if (!IsLocationSimilar(consultation.Region, expert.Region))
                {
                    isLocationMatch = false;
                    mismatchField = "регионе/области";
                }
            }

            if (isLocationMatch && !string.IsNullOrWhiteSpace(consultation.City) && !string.IsNullOrWhiteSpace(expert.City))
            {
                if (!IsLocationSimilar(consultation.City, expert.City))
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

            var dateOnlyUtc = scheduledDateUtc.Date;

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

            consultation.Status = "pending";
            consultation.ScheduledDate = scheduledDateUtc;
            consultation.CreatedAt = DateTime.UtcNow;

            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConsultation), new { id = consultation.Id }, consultation);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateRequest request)
        {
            if (string.IsNullOrEmpty(request.Status))
            {
                return BadRequest(new { message = "Статус не указан." });
            }

            var allowedStatuses = new[] { "confirmed", "cancelled", "completed" };
            if (!allowedStatuses.Contains(request.Status.ToLower()))
            {
                return BadRequest(new { message = "Недопустимый статус. Используйте: confirmed, cancelled, completed." });
            }

            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation == null)
            {
                return NotFound(new { message = "Консультация не найдена." });
            }
            
            if (consultation.Status == "cancelled" && request.Status != "cancelled")
            {
                 return BadRequest(new { message = "Нельзя изменить статус отмененной консультации." });
            }

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

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserConsultations(int userId)
        {
            var consultations = await _context.Consultations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.ScheduledDate)
                .ToListAsync();

            return Ok(consultations);
        }

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

        private bool IsLocationSimilar(string location1, string location2)
        {
            if (string.IsNullOrWhiteSpace(location1) || string.IsNullOrWhiteSpace(location2))
                return false;

            var normalized1 = NormalizeLocation(location1);
            var normalized2 = NormalizeLocation(location2);

            if (normalized1 == normalized2)
                return true;

            double similarity = CalculateSimilarity(normalized1, normalized2);
            
            return similarity >= 0.7;
        }

        private string NormalizeLocation(string location)
        {
            if (string.IsNullOrWhiteSpace(location)) return string.Empty;

            var normalized = location.Trim().ToLowerInvariant();

            var wordsToRemove = new[] { "республика", "федерация", "область", "край", "автономный округ", "г.", "город" };
            foreach (var word in wordsToRemove)
            {
                normalized = normalized.Replace(word, "").Trim();
            }

            normalized = string.Join(" ", normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            if (normalized == "российская" || normalized == "рф") normalized = "россия";
            if (normalized == "санкт петербург" || normalized == "питер") normalized = "санкт-петербург";

            return normalized;
        }

        private double CalculateSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0;

            var words1 = s1.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            var words2 = s2.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

            if (words1.Length == 0 || words2.Length == 0)
                return 0;

            int matches = 0;
            foreach (var word1 in words1)
            {
                foreach (var word2 in words2)
                {
                    if (word1.Contains(word2) || word2.Contains(word1))
                    {
                        matches++;
                        break;
                    }
                }
            }

            double maxWords = Math.Max(words1.Length, words2.Length);
            return matches / maxWords;
        }
    }
}