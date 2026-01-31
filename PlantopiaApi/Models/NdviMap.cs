using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantopiaApi.Models
{
    [Table("ndvi_maps")]
    public class NdviMap
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("date_taken")]
        public DateTime DateTaken { get; set; }

        [Column("map_url")]
        public string? MapUrl { get; set; }

        [Column("min_ndvi_value")]
        public decimal MinNdviValue { get; set; }

        [Column("max_ndvi_value")]
        public decimal MaxNdviValue { get; set; }

        [Column("avg_ndvi_value")]
        public decimal AvgNdviValue { get; set; }

        [Column("cloud_filter_applied")]
        public bool CloudFilterApplied { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
    }
}