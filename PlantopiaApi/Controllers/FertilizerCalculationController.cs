
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;

namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/fertilizer-calculation")]
    public class FertilizerCalculationController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;

        public FertilizerCalculationController(PlantopiaDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<FertilizerCalculation>> CreateCalculation([FromBody] FertilizerCalculation request)
        {
            if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) || 
                !int.TryParse(userIdHeader, out var userId))
            {
                return BadRequest("Требуется заголовок X-User-Id с идентификатором пользователя");
            }

            // Добавляем целевую урожайность из запроса
            var calculation = new FertilizerCalculation
            {
                UserId = userId,
                CropId = request.CropId,
                SoilId = request.SoilId,
                TargetYieldKgHa = request.TargetYieldKgHa, // ← добавлено
                FieldAreaHa = request.FieldAreaHa,
                RecommendedNKgHa = request.RecommendedNKgHa,
                RecommendedPKgHa = request.RecommendedPKgHa,
                RecommendedKKgHa = request.RecommendedKKgHa,
                CalculatedAt = DateTime.UtcNow
            };

            _context.FertilizerCalculations.Add(calculation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCalculation), new { id = calculation.Id }, calculation);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FertilizerCalculation>> GetCalculation(int id)
        {
            var calculation = await _context.FertilizerCalculations.FindAsync(id);
            if (calculation == null)
            {
                return NotFound();
            }
            return calculation;
        }

        [HttpGet("user")]
        public async Task<ActionResult<List<FertilizerCalculation>>> GetUserCalculations()
        {
            if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) || 
                !int.TryParse(userIdHeader, out var userId))
            {
                return BadRequest("Требуется заголовок X-User-Id с идентификатором пользователя");
            }

            var calculations = await _context.FertilizerCalculations
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return calculations;
        }
    }
}