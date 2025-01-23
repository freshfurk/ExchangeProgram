using ExchangeProgram.Data;
using ExchangeProgram.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExchangeProgram.Pages
{
    public class ApplyModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ApplyModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Properties for Binding
        [BindProperty]
        public string FirstName { get; set; }

        [BindProperty]
        public string LastName { get; set; }

        [BindProperty]
        public string BirthDate { get; set; }

        [BindProperty]
        public string Gender { get; set; }

        [BindProperty]
        public string Nationality { get; set; }

        [BindProperty]
        public string ContactEmail { get; set; }

        [BindProperty]
        public string PhoneNumber { get; set; }

        [BindProperty]
        public string Address { get; set; }

        [BindProperty]
        public string HouseNumber { get; set; }

        [BindProperty]
        public string City { get; set; }

        [BindProperty]
        public string Country { get; set; }

        [BindProperty]
        public string UniversityName { get; set; }

        [BindProperty]
        public string Degree { get; set; }

        [BindProperty]
        public string StudyField { get; set; }

        [BindProperty]
        public string MatriculationNumber { get; set; }

        [BindProperty]
        public IFormFile CV { get; set; }

        [BindProperty]
        public IFormFile Transcript { get; set; }

        [BindProperty]
        public IFormFile MotivationLetter { get; set; }

        [BindProperty]
        public IFormFile LanguageCertificate { get; set; }

        [BindProperty]
        public List<IFormFile> SupportingDocuments { get; set; }

        [BindProperty]
        public int SelectedProgram { get; set; }

        public List<Programs> Programs { get; set; }

        // OnGet: Load available programs and prefill existing data
        public IActionResult OnGet()
        {
            // Nur Programme mit gültiger Deadline laden
            Programs = _context.Programs
                .Where(p => p.Deadline > DateTime.Now) // Programme filtern, deren Deadline noch nicht überschritten ist
                .ToList();

            if (!Request.Query.ContainsKey("id") || !int.TryParse(Request.Query["id"], out var studentId))
            {
                TempData["ErrorMessage"] = "You must be logged in to apply.";
                return RedirectToPage("/LoginRegister");
            }

            var student = _context.Students.Include(s => s.Applications)
                .ThenInclude(a => a.Documents)
                .FirstOrDefault(s => s.Id == studentId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToPage("/LoginRegister");
            }

            // Vorausfüllen der Felder mit den letzten Bewerbungsdaten (falls vorhanden)
            var application = student.Applications?.OrderByDescending(a => a.ApplicationDate).FirstOrDefault();
            if (application != null)
            {
                FirstName = application.FirstName;
                LastName = application.LastName;
                BirthDate = application.BirthDate;
                Gender = application.Gender;
                Nationality = application.Nationality;
                ContactEmail = application.ContactEmail;
                PhoneNumber = application.PhoneNumber;
                Address = application.Address;
                HouseNumber = application.HouseNumber;
                City = application.City;
                Country = application.Country;
                UniversityName = application.UniversityName;
                Degree = application.Degree;
                StudyField = application.StudyField;
                MatriculationNumber = application.MatriculationNumber;
                SelectedProgram = (int)application.ProgramId;

                // Dokumente aus der letzten Bewerbung laden
                if (application.Documents != null)
                {
                    // Beispiel: Laden der Dateinamen für die Anzeige
                    ViewData["CV"] = application.Documents.FirstOrDefault(d => d.FileType == "CV")?.FileName;
                    ViewData["Transcript"] = application.Documents.FirstOrDefault(d => d.FileType == "Transcript")?.FileName;
                    ViewData["MotivationLetter"] = application.Documents.FirstOrDefault(d => d.FileType == "Motivation Letter")?.FileName;
                    ViewData["LanguageCertificate"] = application.Documents.FirstOrDefault(d => d.FileType == "Language Certificate")?.FileName;
                    ViewData["SupportingDocuments"] = application.Documents
                        .Where(d => d.FileType == "Supporting Document")
                        .Select(d => d.FileName)
                        .ToList();
                }
            }
            else
            {
                // Felder leer lassen, wenn keine Bewerbung existiert
                FirstName = student.FirstName;
                LastName = student.LastName;
                ContactEmail = student.Email;
            }

            return Page();
        }

        public IActionResult OnPostSubmit()
        {
            // Retrieve the StudentId from the hidden input field
            if (!Request.Form.ContainsKey("StudentId") || !int.TryParse(Request.Form["StudentId"], out var studentId))
            {
                TempData["ErrorMessage"] = "You must be logged in to apply.";
                return RedirectToPage("/LoginRegister");
            }

            // Retrieve the student from the database
            var student = _context.Students.Include(s => s.Applications).FirstOrDefault(s => s.Id == studentId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToPage("/LoginRegister");
            }

            // Check if the student already has an application
            var application = student.Applications?.FirstOrDefault();
            if (application == null)
            {
                // Create a new application
                application = new Application
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    BirthDate = BirthDate,
                    Gender = Gender,
                    Nationality = Nationality,
                    ContactEmail = ContactEmail,
                    PhoneNumber = PhoneNumber,
                    Address = Address,
                    HouseNumber = HouseNumber,
                    City = City,
                    Country = Country,
                    UniversityName = UniversityName,
                    Degree = Degree,
                    StudyField = StudyField,
                    MatriculationNumber = MatriculationNumber,
                    ProgramId = SelectedProgram,
                    StudentId = studentId,
                    ApplicationDate = DateTime.Now,
                    Status = "Pending"
                };

                _context.Applications.Add(application);
            }
            else
            {
                // Update the existing application
                application.FirstName = FirstName;
                application.LastName = LastName;
                application.BirthDate = BirthDate;
                application.Gender = Gender;
                application.Nationality = Nationality;
                application.ContactEmail = ContactEmail;
                application.PhoneNumber = PhoneNumber;
                application.Address = Address;
                application.HouseNumber = HouseNumber;
                application.City = City;
                application.Country = Country;
                application.UniversityName = UniversityName;
                application.Degree = Degree;
                application.StudyField = StudyField;
                application.MatriculationNumber = MatriculationNumber;
                application.ProgramId = SelectedProgram;
                application.ApplicationDate = DateTime.Now; // Update the application date to reflect changes
            }

            _context.SaveChanges(); // Save changes to ensure ApplicationId is generated

            // Handle document uploads and associate them with the application
            var documents = new List<Document>();

            if (CV != null) documents.Add(CreateDocument(CV, "CV", studentId, application.Id));
            if (Transcript != null) documents.Add(CreateDocument(Transcript, "Transcript", studentId, application.Id));
            if (MotivationLetter != null) documents.Add(CreateDocument(MotivationLetter, "Motivation Letter", studentId, application.Id));
            if (LanguageCertificate != null) documents.Add(CreateDocument(LanguageCertificate, "Language Certificate", studentId, application.Id));

            if (SupportingDocuments != null && SupportingDocuments.Any())
            {
                foreach (var doc in SupportingDocuments)
                {
                    documents.Add(CreateDocument(doc, "Supporting Document", studentId, application.Id));
                }
            }

            if (documents.Any())
            {
                // Remove old documents associated with this application
                var existingDocuments = _context.Documents.Where(d => d.ApplicationId == application.Id).ToList();
                _context.Documents.RemoveRange(existingDocuments);

                // Add new documents
                _context.Documents.AddRange(documents);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Your application has been submitted successfully!";
            return RedirectToPage("/Index", new { id = studentId });
        }


        private Document CreateDocument(IFormFile file, string type, int studentId, int applicationId)
        {
            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            return new Document
            {
                FileName = file.FileName,
                FileType = type,
                FileData = memoryStream.ToArray(),
                StudentId = studentId,
                ApplicationId = applicationId
            };
        }
    }
}
