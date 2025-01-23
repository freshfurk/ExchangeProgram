namespace ExchangeProgram.Models
{
    public class Application
    {
        public int Id { get; set; } // Primärschlüssel

        // Persönliche Informationen
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Nationality { get; set; }

        // Kontaktinformationen
        public string? ContactEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? HouseNumber { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        // Akademische Informationen
        public string? UniversityName { get; set; }
        public string? Degree { get; set; }
        public string? StudyField { get; set; }
        public string? MatriculationNumber { get; set; }

        // Hochgeladene Dateien
        public byte[]? ProfilePicture { get; set; }
        public ICollection<Document>? Documents { get; set; }

        // Navigation zu Programmen
        public int? ProgramId { get; set; }
        public Programs? Program { get; set; }

        // Navigation zu Studenten
        public int? StudentId { get; set; }
        public Student? Student { get; set; }

        // Metadaten
        public DateTime? ApplicationDate { get; set; }
        public string? Status { get; set; } // z. B. "Pending", "Approved", "Rejected"
    }
}
