using System.ComponentModel.DataAnnotations; 
namespace PlantopiaApi.Units;


public class CreateUserTaskRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(4000)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public string? Category { get; set; }

    public bool? Completed { get; set; } // nullable, чтобы можно было не передавать
}