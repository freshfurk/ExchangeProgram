namespace ExchangeProgram.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; } // Motivation Letter, Transcript, CV
        public byte[] FileData { get; set; } // Datei als BLOB speichern
        public int StudentId { get; set; } // Verknüpfung mit dem Studenten
        public Student Student { get; set; }
    }
}
