using System.Text.Json.Serialization;
public class LoginResponse
{
    public string SessionId { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public bool SubscriptionStatus { get; set; }
    public string UserRole { get; set; }
    
    public string ApiKey { get; set; }
    [JsonPropertyName("ndvi_api_key")] 
    public string? NDVIApiKey { get; set; }
    // --- НОВЫЕ ПОЛЯ ДЛЯ ЭКСПЕРТА ---
    public string? Specialization { get; set; }
    public int? ExperienceYears { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? City { get; set; }
}