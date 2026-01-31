using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantopiaApi.Models
{
    [Table("soil_tests")]
    public class SoilTest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("date_taken")]
        public DateTime DateTaken { get; set; }

        [Column("ph_level")]
        public decimal PhLevel { get; set; }

        [Column("nitrate_ppm")]
        public decimal NitratePpm { get; set; }

        [Column("recommended_lime_kg_ha")]
        public decimal RecommendedLimeKgHa { get; set; }

        [Column("recommended_fertilizer_kg_ha")]
        public decimal RecommendedFertilizerKgHa { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
    }
}