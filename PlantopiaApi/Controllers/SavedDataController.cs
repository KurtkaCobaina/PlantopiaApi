// Controllers/SavedDataController.cs
using Microsoft.AspNetCore.Mvc;
using PlantopiaApi.Data;
using PlantopiaApi.Models;

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

    // === FERTILIZER CALCULATIONS ===

    [HttpGet("savedfertilizer")]
    public IActionResult GetSavedFertilizer([FromQuery] int userId)
    {
        var calculations = _context.FertilizerCalculations
            .Where(f => f.UserId == userId)
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