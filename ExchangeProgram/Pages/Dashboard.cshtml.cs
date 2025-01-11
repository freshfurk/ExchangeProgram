using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExchangeProgram.Models;
using ExchangeProgram.Data;
using Microsoft.VisualBasic.FileIO;

namespace ExchangeProgram.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Student Student { get; set; }

        [BindProperty]
        public IFormFile File { get; set; }

        [BindProperty]
        public string FileType { get; set; }

        public List<Document> Documents { get; set; }

        public IActionResult OnGet(int id)
        {
            if (id == 0)
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToPage("/Index");
            }

            // Benutzer laden und TempData setzen
            Student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (Student == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage("/Index");
            }

            // Zugriff nur fÃ¼r Studenten
            if (!Student.isStudent)
            {
                TempData["ErrorMessage"] = "Unauthorized access.";
                return RedirectToPage("/OrganizerDashboard", new { id });
            }

            Documents = _context.Documents.Where(d => d.StudentId == id).ToList();

            TempData["UserId"] = id; // Id in TempData speichern
            return Page();
        }

        public IActionResult OnPostSaveProfile(IFormFile ProfilePicture)
        {
            // Id aus TempData abrufen
            if (!TempData.ContainsKey("UserId") || Convert.ToInt32(TempData["UserId"]) == 0)
            {
                TempData["ErrorMessage"] = "User ID not found.";
                return RedirectToPage("/Index");
            }

            int id = Convert.ToInt32(TempData["UserId"]);

            var existingStudent = _context.Students.FirstOrDefault(s => s.Id == id);
            if (existingStudent == null)
            {
                TempData["ErrorMessage"] = "User not found in the database.";
                return RedirectToPage("/Index");
            }

            // Profildaten aktualisieren
            existingStudent.FirstName = Student.FirstName;
            existingStudent.LastName = Student.LastName;
            existingStudent.BirthDate = Student.BirthDate;
            existingStudent.Address = Student.Address;
            existingStudent.HouseNumber = Student.HouseNumber;
            existingStudent.City = Student.City;
            existingStudent.Country = Student.Country;
            existingStudent.PhoneNumber = Student.PhoneNumber;
            existingStudent.MatriculationNumber = Student.MatriculationNumber;
            existingStudent.UniversityName = Student.UniversityName;
            existingStudent.StudyField = Student.StudyField;
            existingStudent.Degree = Student.Degree;

            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                ProfilePicture.CopyTo(memoryStream);

                // Optional: Bildvalidierung hinzufügen
                if (ProfilePicture.ContentType == "image/jpeg" || ProfilePicture.ContentType == "image/png")
                {
                    existingStudent.ProfilePicture = memoryStream.ToArray();
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid image format. Only JPEG and PNG are allowed.";
                    return RedirectToPage("/Dashboard", new { id });
                }
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Profile updated successfully!";

            return RedirectToPage("/Dashboard", new { id });
        }

        public IActionResult OnPostUploadDocument()
        {
            if (!TempData.ContainsKey("UserId") || Convert.ToInt32(TempData["UserId"]) == 0)
            {
                TempData["ErrorMessage"] = "User ID not found.";
                return RedirectToPage("/Index");
            }

            int id = Convert.ToInt32(TempData["UserId"]);

            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToPage("/Index");
            }

            if (File != null)
            {
                using var memoryStream = new MemoryStream();
                File.CopyTo(memoryStream);
                var fileData = memoryStream.ToArray();

                var document = new Document
                {
                    FileName = File.FileName,
                    FileType = FileType,
                    FileData = fileData,
                    StudentId = id // Sicherstellen, dass die ID existiert
                };

                _context.Documents.Add(document);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Document uploaded successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "No file selected.";
            }

            TempData["ActiveTab"] = "documents"; // Dokumenten-Tab aktiv halten
            return RedirectToPage("/Dashboard", new { id });
        }

        public IActionResult OnGetDownloadDocument(int documentId)
        {
            var document = _context.Documents.FirstOrDefault(d => d.Id == documentId);
            if (document != null)
            {
                return File(document.FileData, "application/octet-stream", document.FileName);
            }
            return NotFound();
        }

        public IActionResult OnPostDeleteDocument(int documentId)
        {
            if (!TempData.ContainsKey("UserId") || Convert.ToInt32(TempData["UserId"]) == 0)
            {
                TempData["ErrorMessage"] = "User ID not found.";
                return RedirectToPage("/Index");
            }

            int id = Convert.ToInt32(TempData["UserId"]);

            var document = _context.Documents.FirstOrDefault(d => d.Id == documentId);
            if (document != null)
            {
                _context.Documents.Remove(document);
                _context.SaveChanges();
            }

            TempData["ActiveTab"] = "documents"; // Dokumenten-Tab aktiv halten
            return RedirectToPage("/Dashboard", new { id });
        }
    }
}
