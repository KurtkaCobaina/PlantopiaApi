// Models/SoilType.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantopiaApi.Models
{
    [Table("soil_types")]
    public class SoilType
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("ph_level_min")]
        public decimal? PhLevelMin { get; set; }

        [Column("ph_level_max")]
        public decimal? PhLevelMax { get; set; }

        [Column("n_correction_factor")]
        public decimal NCorrectionFactor { get; set; } = 1.0m;

        [Column("p_correction_factor")]
        public decimal PCorrectionFactor { get; set; } = 1.0m;

        [Column("k_correction_factor")]
        public decimal KCorrectionFactor { get; set; } = 1.0m;
    }
}