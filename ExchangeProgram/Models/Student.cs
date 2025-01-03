namespace ExchangeProgram.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public string MatriculationNumber { get; set; }
        public string PasswordHash { get; set; } // Für Passwörter
    }
}
