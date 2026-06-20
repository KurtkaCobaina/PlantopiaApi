namespace PlantopiaApi.Units;

public class NdviMapRequest
{
    public int UserId { get; set; }
    public DateTime DateTaken { get; set; }
    public string? MapUrl { get; set; }
    public double MinNdviValue { get; set; }
    public double MaxNdviValue { get; set; }
    public double AvgNdviValue { get; set; }
    public bool CloudFilterApplied { get; set; }
}