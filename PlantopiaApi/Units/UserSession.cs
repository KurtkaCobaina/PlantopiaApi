public class UserSession
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public bool SubscriptionStatus { get; set; }
    public string UserRole { get; set; }
    public string ApiKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    
}