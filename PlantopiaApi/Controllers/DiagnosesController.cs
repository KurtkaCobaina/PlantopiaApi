using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;
using System.Text.Json;

namespace PlantopiaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosesController : ControllerBase
{
    private readonly PlantopiaDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DiagnosesController(PlantopiaDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost("save-diagnosis")]
    public async Task<IActionResult> SaveDiagnosis([FromBody] JsonElement requestBody)
    {
        try
        {
            if (!requestBody.TryGetProperty("userId", out var userIdEl) ||
                !userIdEl.TryGetInt32(out var userId) || userId <= 0)
            {
                return BadRequest("Valid 'userId' (positive integer) is required.");
            }

            if (!requestBody.TryGetProperty("imageUrl", out var imageUrlEl))
            {
                return BadRequest("'imageUrl' is required.");
            }
            string imageUrl = imageUrlEl.GetString()?.Trim() ?? "";
            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest("'imageUrl' cannot be empty.");
            }

            if (!requestBody.TryGetProperty("result", out var resultEl))
            {
                return BadRequest("'result' is required.");
            }

            if (!resultEl.TryGetProperty("classification", out var classificationEl) ||
                !classificationEl.TryGetProperty("suggestions", out var suggestionsArray))
            {
                return BadRequest("Invalid Plant.id response: missing classification.suggestions.");
            }

            var suggestions = suggestionsArray.EnumerateArray().ToList();
            if (!suggestions.Any())
            {
                return BadRequest("No plant suggestions found in classification.");
            }

            var topSuggestion = suggestions.First();
            string plantName = topSuggestion.GetProperty("name").GetString() ?? "Unknown";
            double confidence = topSuggestion.GetProperty("probability").GetDouble();

            string? commonNames = null;
            if (topSuggestion.TryGetProperty("details", out var detailsEl) &&
                detailsEl.TryGetProperty("common_names", out var commonNamesEl))
            {
                var names = commonNamesEl.EnumerateArray()
                    .Select(el => el.GetString())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();
                if (names.Length > 0)
                    commonNames = string.Join(", ", names);
            }

            bool issuesDetected = false;
            string? diseaseDetails = null;

            if (resultEl.TryGetProperty("is_healthy", out var healthyEl))
            {
                bool isHealthy = healthyEl.GetProperty("binary").GetBoolean();
                issuesDetected = !isHealthy;

                if (issuesDetected && resultEl.TryGetProperty("disease", out var diseaseEl) &&
                    diseaseEl.TryGetProperty("suggestions", out var diseaseSuggestions))
                {
                    var diseases = diseaseSuggestions.EnumerateArray()
                        .Select(el => new
                        {
                            Name = el.GetProperty("name").GetString(),
                            Probability = el.GetProperty("probability").GetDouble()
                        })
                        .Where(d => d.Name != null)
                        .ToList();

                    if (diseases.Any())
                    {
                        diseaseDetails = JsonSerializer.Serialize(diseases, new JsonSerializerOptions
                        {
                            WriteIndented = false
                        });
                    }
                }
            }

            var diagnosis = new Diagnosis
            {
                UserId = userId,
                ImageUrl = imageUrl,
                PlantName = plantName,
                CommonNames = commonNames,
                Confidence = (decimal)confidence,
                IssuesDetected = issuesDetected,
                DiseaseDetails = diseaseDetails,
                CreatedAt = DateTime.UtcNow
            };

            _context.Diagnoses.Add(diagnosis);
            await _context.SaveChangesAsync();

            return Ok(new { id = diagnosis.Id, message = "Diagnosis saved successfully." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Save diagnosis error: {ex}");
            return StatusCode(500, "An internal error occurred while saving the diagnosis.");
        }
    }
}