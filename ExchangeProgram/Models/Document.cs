namespace ExchangeProgram.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; } // Motivation Letter, Transcript, CV
        public byte[]? FileData { get; set; } // Datei als BLOB speichern

        // Verknüpfung mit Student
        public int? StudentId { get; set; } // Optional, falls ein Dokument zu einer Bewerbung gehört
        public Student? Student { get; set; }

        // Verknüpfung mit Application
        public int? ApplicationId { get; set; } // Optional
        public Application? Application { get; set; }
    }
}
