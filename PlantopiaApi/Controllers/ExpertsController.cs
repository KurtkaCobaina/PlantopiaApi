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
    }
}