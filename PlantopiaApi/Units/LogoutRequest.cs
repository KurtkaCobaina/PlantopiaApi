using System.ComponentModel.DataAnnotations;

public class LogoutRequest
{
    [Required]
    public string SessionId { get; set; }
}