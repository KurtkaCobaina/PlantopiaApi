using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantopiaApi.Models
{
    [Table("consultations")]
    public class Consultation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("expert_id")]
        public int ExpertId { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("country")]
        [MaxLength(4000)]
        public string? Country { get; set; }

        [Column("region")]
        [MaxLength(4000)]
        public string? Region { get; set; }

        [Column("city")]
        [MaxLength(4000)]
        public string? City { get; set; }

        [Column("village")]
        [MaxLength(4000)]
        public string? Village { get; set; }

        [Column("street_address")]
        public string? StreetAddress { get; set; }

        [Column("scheduled_date")]
        public DateTime ScheduledDate { get; set; }

        [Column("status")]
        [MaxLength(4000)]
        public string? Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
        public virtual Expert? Expert { get; set; }
        public virtual ICollection<Diagnosis>? Diagnoses { get; set; }
    }
}