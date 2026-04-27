

// Units/FertilizerCalculationDto.cs
namespace PlantopiaApi.Units
{
    public class FertilizerCalculationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        // IDs
        public int? CropId { get; set; }
        public int? SoilId { get; set; }
        
        // Названия (новые поля для фронта)
        public string? CropName { get; set; }
        public string? SoilName { get; set; }
        
        // Данные расчета
        public decimal? TargetYieldKgHa { get; set; }
        public decimal? FieldAreaHa { get; set; }
        public decimal? RecommendedNKgHa { get; set; }
        public decimal? RecommendedPKgHa { get; set; }
        public decimal? RecommendedKKgHa { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}