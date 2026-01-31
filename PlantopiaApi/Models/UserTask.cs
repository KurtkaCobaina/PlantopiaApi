using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantopiaApi.Models
{
    [Table("tasks")]
    public class UserTask
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("title")]
        [MaxLength(4000)]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("due_date")]
        public DateTime DueDate { get; set; }

        [Column("completed")]
        public bool Completed { get; set; } = false;

        [Column("category")]
        [MaxLength(4000)]
        public string? Category { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
    }
}