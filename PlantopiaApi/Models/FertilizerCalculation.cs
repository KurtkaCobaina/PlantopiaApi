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

        [Column("crop_id")]
        public int? CropId { get; set; }

        [Column("soil_id")]
        public int? SoilId { get; set; }

        [Column("target_yield_kg_ha")]
        public decimal? TargetYieldKgHa { get; set; }

        [Column("field_area_ha")]
        public decimal? FieldAreaHa { get; set; }

        [Column("recommended_n_kg_ha")]
        public decimal? RecommendedNKgHa { get; set; }

        [Column("recommended_p_kg_ha")]
        public decimal? RecommendedPKgHa { get; set; }

        [Column("recommended_k_kg_ha")]
        public decimal? RecommendedKKgHa { get; set; }

        [Column("calculated_at")]
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
    }
}