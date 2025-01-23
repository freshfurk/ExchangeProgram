namespace ExchangeProgram.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        //public DateTime? BirthDate { get; set; } // Datum als DateTime für bessere Verarbeitung
        //public string? MatriculationNumber { get; set; }
        public string PasswordHash { get; set; } // Für Passwörter

        // Kontaktinformationen
        //public string? Address { get; set; }
        //public string? HouseNumber { get; set; }
        //public string? City { get; set; }
        //public string? Country { get; set; }
        //public string? PhoneNumber { get; set; }

        // Akademische Informationen
        //public string? UniversityName { get; set; }
        //public string? StudyField { get; set; }
        //public string? Degree { get; set; }
        //public string? Gender { get; set; }
        //public string? Nationality { get; set; }

        // Navigation Properties
        public ICollection<Document>? Documents { get; set; }
        public ICollection<Application>? Applications { get; set; } // Bewerbungen des Studenten

        // Profilbild
        //public byte[]? ProfilePicture { get; set; }

        // Rolle
        public bool isStudent { get; set; }
    }
}
