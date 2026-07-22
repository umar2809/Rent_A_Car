public class Car
{
    public int Id { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string LicensePlate { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsAvailable { get; set; } = true;
}