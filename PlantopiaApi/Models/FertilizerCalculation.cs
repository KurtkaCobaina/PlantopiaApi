using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantopiaApi.Models
{
    [Table("fertilizer_calculations")]
    public class FertilizerCalculation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("crop_type")]
        [MaxLength(4000)]
        public string? CropType { get; set; }

        [Column("soil_type")]
        [MaxLength(4000)]
        public string? SoilType { get; set; }

        [Column("target_yield_ton_ha")]
        public decimal TargetYieldTonHa { get; set; }

        [Column("field_area_ha")]
        public decimal FieldAreaHa { get; set; }

        [Column("recommended_n_kg_ha")]
        public decimal RecommendedNKgHa { get; set; }

        [Column("recommended_p_kg_ha")]
        public decimal RecommendedPKgHa { get; set; }

        [Column("recommended_k_kg_ha")]
        public decimal RecommendedKKgHa { get; set; }

        [Column("calculated_at")]
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
    }
}