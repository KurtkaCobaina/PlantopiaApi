using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;
using System.Text.RegularExpressions;

namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgroDataController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;

        public AgroDataController(PlantopiaDbContext context)
        {
            _context = context;
        }

        [HttpGet("crops")]
        public async Task<ActionResult<List<Crop>>> GetCrops()
        {
            return await _context.Crops.ToListAsync();
        }

        [HttpPost("crops")]
        public async Task<ActionResult<Crop>> CreateCrop([FromBody] Crop crop)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var nameRegex = @"^[а-яА-ЯёЁa-zA-Z\s]+$";
            
            if (string.IsNullOrWhiteSpace(crop.Name) || !Regex.IsMatch(crop.Name.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Название культуры должно содержать только буквы и пробелы." });
            }

            _context.Crops.Add(crop);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCrops), new { id = crop.Id }, crop);
        }

        [HttpPut("crops/{id}")]
        public async Task<IActionResult> UpdateCrop(int id, [FromBody] Crop crop)
        {
            if (id != crop.Id)
                return BadRequest("ID в URL не совпадает с ID в теле запроса");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var nameRegex = @"^[а-яА-ЯёЁa-zA-Z\s]+$";
            
            if (string.IsNullOrWhiteSpace(crop.Name) || !Regex.IsMatch(crop.Name.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Название культуры должно содержать только буквы и пробелы." });
            }

            _context.Entry(crop).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CropExists(id))
                    return NotFound("Культура не найдена");
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("crops/{id}")]
        public async Task<IActionResult> DeleteCrop(int id)
        {
            var crop = await _context.Crops.FindAsync(id);
            if (crop == null)
                return NotFound("Культура не найдена");

            _context.Crops.Remove(crop);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CropExists(int id)
        {
            return _context.Crops.Any(e => e.Id == id);
        }

        [HttpGet("soil-types")]
        public async Task<ActionResult<List<SoilType>>> GetSoilTypes()
        {
            return await _context.SoilTypes.ToListAsync();
        }

        [HttpPost("soil-types")]
        public async Task<ActionResult<SoilType>> CreateSoilType([FromBody] SoilType soilType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var nameRegex = @"^[а-яА-ЯёЁa-zA-Z\s]+$";
            
            if (string.IsNullOrWhiteSpace(soilType.Name) || !Regex.IsMatch(soilType.Name.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Название типа почвы должно содержать только буквы и пробелы." });
            }

            _context.SoilTypes.Add(soilType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSoilTypes), new { id = soilType.Id }, soilType);
        }

        [HttpPut("soil-types/{id}")]
        public async Task<IActionResult> UpdateSoilType(int id, [FromBody] SoilType soilType)
        {
            if (id != soilType.Id)
                return BadRequest("ID в URL не совпадает с ID в теле запроса");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var nameRegex = @"^[а-яА-ЯёЁa-zA-Z\s]+$";
            
            if (string.IsNullOrWhiteSpace(soilType.Name) || !Regex.IsMatch(soilType.Name.Trim(), nameRegex))
            {
                return BadRequest(new { message = "Название типа почвы должно содержать только буквы и пробелы." });
            }

            _context.Entry(soilType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SoilTypeExists(id))
                    return NotFound("Тип почвы не найден");
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("soil-types/{id}")]
        public async Task<IActionResult> DeleteSoilType(int id)
        {
            var soilType = await _context.SoilTypes.FindAsync(id);
            if (soilType == null)
                return NotFound("Тип почвы не найден");

            _context.SoilTypes.Remove(soilType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SoilTypeExists(int id)
        {
            return _context.SoilTypes.Any(e => e.Id == id);
        }
    }
}