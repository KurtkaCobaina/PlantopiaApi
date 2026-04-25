using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantopiaApi.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("email")]
        [MaxLength(4000)]
        public string? Email { get; set; }

        [Column("user_password")]
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

        [Column("subscription_status")]
        public bool SubscriptionStatus { get; set; } = false;

        [Column("user_role")]
        [MaxLength(4000)]
        public string? UserRole { get; set; }

        [Column("api_key")]
        [MaxLength(4000)]
        public string? ApiKey { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Column("ndvi_api_key")]
        [MaxLength(4000)]
        public string? NDVIApiKey { get; set; }
        // Навигационные свойства — БЕЗ ConsultationsAsExpert
   
        public virtual ICollection<Consultation>? ConsultationsAsUser { get; set; }
        public virtual ICollection<Diagnosis>? Diagnoses { get; set; }
        public virtual ICollection<FertilizerCalculation>? FertilizerCalculations { get; set; }
        public virtual ICollection<NdviMap>? NdviMaps { get; set; }
        public virtual ICollection<SoilTest>? SoilTests { get; set; }
        public virtual ICollection<UserTask>? UserTasks { get; set; }
    }
}