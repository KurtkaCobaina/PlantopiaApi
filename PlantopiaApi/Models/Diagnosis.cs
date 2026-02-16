using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantopiaApi.Models
{
    [Table("diagnoses")]
    public class Diagnosis
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("plant_name")]
        [MaxLength(4000)]
        public string? PlantName { get; set; }

        [Column("common_names")]
        public string? CommonNames { get; set; }

        [Column("confidence")]
        public decimal Confidence { get; set; }

        [Column("issues_detected")]
        public bool IssuesDetected { get; set; }

        [Column("disease_details")]
        public string? DiseaseDetails { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
       
    }
}