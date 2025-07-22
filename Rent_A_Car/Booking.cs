public class Booking
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public int CustomerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalCost { get; set; }
    public bool IsCompleted { get; set; }

    // Add navigation properties for EF Core
    public Car? Car { get; set; }
        public Customer? Customer { get; set; }
    }