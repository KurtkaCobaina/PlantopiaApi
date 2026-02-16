// Models/Crop.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantopiaApi.Models
{
    [Table("crops")]
    public class Crop
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("scientific_name")]
        public string ScientificName { get; set; } = string.Empty;

        [Column("optimal_n_g_1m2_min")]
        public decimal OptimalNG1m2Min { get; set; }

        [Column("optimal_n_g_1m2_max")]
        public decimal OptimalNG1m2Max { get; set; }

        [Column("optimal_p_g_1m2_min")]
        public decimal OptimalPG1m2Min { get; set; }

        [Column("optimal_p_g_1m2_max")]
        public decimal OptimalPG1m2Max { get; set; }

        [Column("optimal_k_g_1m2_min")]
        public decimal OptimalKG1m2Min { get; set; }

        [Column("optimal_k_g_1m2_max")]
        public decimal OptimalKG1m2Max { get; set; }

        [Column("typical_yield_kg_1m2")]
        public decimal? TypicalYieldKg1m2 { get; set; }

        [Column("growth_period_days")]
        public int GrowthPeriodDays { get; set; }
    }
}