using Microsoft.AspNetCore.Mvc;
using PlantopiaApi.Data;
using PlantopiaApi.Models;
using PlantopiaApi.Units; // <-- Важно: подключаем пространство имен с DTO

namespace PlantopiaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SavedDataController : ControllerBase
{
    private readonly PlantopiaDbContext _context;

    public SavedDataController(PlantopiaDbContext context)
    {
        _context = context;
    }

    // === NDVI MAPS ===

    [HttpGet("savedndvi")]
    public IActionResult GetSavedNdvi([FromQuery] int userId)
    {
        var ndviMaps = _context.NdviMaps
            .Where(map => map.UserId == userId)
            .ToList();
        return Ok(ndviMaps);
    }

    [HttpDelete("ndvi/{id:int}")]
    public IActionResult DeleteNdviMap(int id)
    {
        var userIdStr = Request.Headers["X-User-Id"].FirstOrDefault();
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
        {
            return Unauthorized("Требуется заголовок X-User-Id.");
        }

        var ndviMap = _context.NdviMaps
            .FirstOrDefault(m => m.Id == id && m.UserId == currentUserId);

        if (ndviMap == null)
        {
            return NotFound($"NDVI-карта с ID {id} не найдена или не принадлежит вам.");
        }

        _context.NdviMaps.Remove(ndviMap);
        _context.SaveChanges();
        return NoContent(); // 204
    }

    // === DIAGNOSES ===

    [HttpGet("saveddiagnosis")]
    public IActionResult GetSavedDiagnosis([FromQuery] int userId)
    {
        var diagnoses = _context.Diagnoses
            .Where(d => d.UserId == userId)
            .ToList();
        return Ok(diagnoses);
    }

    [HttpDelete("diagnosis/{id:int}")]
    public IActionResult DeleteDiagnosis(int id)
    {
        var userIdStr = Request.Headers["X-User-Id"].FirstOrDefault();
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
        {
            return Unauthorized("Требуется заголовок X-User-Id.");
        }

        var diagnosis = _context.Diagnoses
            .FirstOrDefault(d => d.Id == id && d.UserId == currentUserId);

        if (diagnosis == null)
        {
            return NotFound($"Диагноз с ID {id} не найден или не принадлежит вам.");
        }

        _context.Diagnoses.Remove(diagnosis);
        _context.SaveChanges();
        return NoContent();
    }

    // === FERTILIZER CALCULATIONS (ОБНОВЛЕНО) ===

    [HttpGet("savedfertilizer")]
    public IActionResult GetSavedFertilizer([FromQuery] int userId)
    {
        // Выполняем Join с таблицами Crops и SoilTypes, чтобы получить названия
        var calculations = _context.FertilizerCalculations
            .Where(f => f.UserId == userId)
            .Join(_context.Crops, 
                calc => calc.CropId, 
                crop => crop.Id, 
                (calc, crop) => new { calc, crop })
            .Join(_context.SoilTypes, 
                joined => joined.calc.SoilId, 
                soil => soil.Id, 
                (joined, soil) => new FertilizerCalculationDto // <-- Возвращаем наш DTO из Units
                {
                    Id = joined.calc.Id,
                    UserId = joined.calc.UserId,
                    CropId = joined.calc.CropId,
                    SoilId = joined.calc.SoilId,
                    
                    // Заполняем названия
                    CropName = joined.crop.Name,       
                    SoilName = soil.Name,              
                    
                    TargetYieldKgHa = joined.calc.TargetYieldKgHa,
                    FieldAreaHa = joined.calc.FieldAreaHa,
                    RecommendedNKgHa = joined.calc.RecommendedNKgHa,
                    RecommendedPKgHa = joined.calc.RecommendedPKgHa,
                    RecommendedKKgHa = joined.calc.RecommendedKKgHa,
                    CalculatedAt = joined.calc.CalculatedAt
                })
            .OrderByDescending(c => c.CalculatedAt) // Опционально: сортировка по дате (новые сверху)
            .ToList();

        return Ok(calculations);
    }

    [HttpDelete("fertilizer/{id:int}")]
    public IActionResult DeleteFertilizerCalculation(int id)
    {
        var userIdStr = Request.Headers["X-User-Id"].FirstOrDefault();
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
        {
            return Unauthorized("Требуется заголовок X-User-Id.");
        }

        var calculation = _context.FertilizerCalculations
            .FirstOrDefault(f => f.Id == id && f.UserId == currentUserId);

        if (calculation == null)
        {
            return NotFound($"Расчёт удобрений с ID {id} не найден или не принадлежит вам.");
        }

        _context.FertilizerCalculations.Remove(calculation);
        _context.SaveChanges();
        return NoContent();
    }
}