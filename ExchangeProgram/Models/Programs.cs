namespace ExchangeProgram.Models
{
    public class Programs
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateOnly? Deadline { get; set; }
    }
}
