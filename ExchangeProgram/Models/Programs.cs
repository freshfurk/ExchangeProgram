namespace ExchangeProgram.Models
{
    public class Programs
    {
        public int Id { get; set; }
        public string? Name { get; set; } // Pflichtfeld
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; } // Datum als DateTime, nicht DateOnly

        // Navigation Property für Bewerbungen
        public ICollection<Application>? Applications { get; set; }
    }
}
