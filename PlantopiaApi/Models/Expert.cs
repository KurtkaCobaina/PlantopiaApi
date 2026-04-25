using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantopiaApi.Models
{
    [Table("experts")]
    public class Expert
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("email")]
        [MaxLength(4000)]
        public string? Email { get; set; }

        // Поле password есть на схеме, добавляем его
        [Column("password")]
        [MaxLength(4000)]
        public string? Password { get; set; }

        [Column("first_name")]
        [MaxLength(4000)]
        public string? FirstName { get; set; }

        [Column("last_name")]
        [MaxLength(4000)]
        public string? LastName { get; set; }

        [Column("phone")]
        [MaxLength(4000)]
        public string? Phone { get; set; }

        [Column("specialization")]
        [MaxLength(4000)]
        public string? Specialization { get; set; }

        [Column("experience_years")]
        public int ExperienceYears { get; set; }

        [Column("hourly_rate")]
        public decimal HourlyRate { get; set; }

        [Column("is_available")]
        public bool IsAvailable { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Дополнительные поля адреса, видимые на схеме
        [Column("country")]
        [MaxLength(4000)]
        public string? Country { get; set; }

        [Column("region")]
        [MaxLength(4000)]
        public string? Region { get; set; }

        [Column("city")]
        [MaxLength(4000)]
        public string? City { get; set; }

        // ВАЖНО: Удаляем или комментируем связь с User, так как user_id нет в БД
        // [Column("user_id")]
        // public int UserId { get; set; }

        // public virtual User? User { get; set; }

        public virtual ICollection<Consultation>? ConsultationsAsExpert { get; set; }
    }
}