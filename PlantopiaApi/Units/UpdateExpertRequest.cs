namespace PlantopiaApi.Units;

public class UpdateExpertRequest
{
    public int UserId { get; set; }
    
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