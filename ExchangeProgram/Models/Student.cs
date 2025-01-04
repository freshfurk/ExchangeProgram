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

        // Neue Felder
        public string? Address { get; set; }
        public string? HouseNumber { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UniversityName { get; set; } // Universitätsname
        public string? StudyField { get; set; } // Studienfach
        public string? Degree { get; set; } // Abschluss

        // Navigation Property
        public ICollection<Document> Documents { get; set; }
    }
}
