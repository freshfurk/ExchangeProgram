using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExchangeProgram.Data;
using ExchangeProgram.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ExchangeProgram.Pages
{
    public class OrganizerDashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public OrganizerDashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Student> Students { get; set; } = new List<Student>();

        [BindProperty]
        public string CurrentEmail { get; set; }

        [BindProperty]
        public string NewEmail { get; set; }

        [BindProperty]
        public string ConfirmEmail { get; set; }

        [BindProperty]
        public string OldPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public string ProgramName { get; set; }

        [BindProperty]
        public string ProgramDescription { get; set; }

        [BindProperty]
        public DateTime ProgramDeadline { get; set; }

        [BindProperty]
        public string ProgramHostUniversityName { get; set; }

        [BindProperty]
        public DateTime ProgramSemesterStart { get; set; }

        [BindProperty]
        public string ProgramCourseOfStudy { get; set; }

        // Properties
        public List<Application> Applications { get; set; } = new();

        public List<Programs> Programs { get; set; } = new();

        [BindProperty]
        public int[] SelectedApplicationIds { get; set; }

        [BindProperty]
        public string ReportType { get; set; }

        [BindProperty]
        public string StatusUpdate { get; set; }

        public IActionResult OnGet()
        {
            // Load all applications with related data
            Applications = _context.Applications
                .Include(a => a.Student)
                .Include(a => a.Program)
                .Include(a => a.Documents)
                .ToList();

            // Load all programs
            Programs = _context.Programs.ToList();

            return Page();
        }

        public IActionResult OnGetDownloadDocument(int documentId)
        {
            var document = _context.Documents.FirstOrDefault(d => d.Id == documentId);
            if (document == null)
            {
                return NotFound("Document not found");
            }

            var fileContent = document.FileData; // Annahme: Das Dokument ist als Byte-Array gespeichert
            var contentType = "application/octet-stream"; // Allgemeiner MIME-Typ; kann angepasst werden
            var fileName = document.FileName;

            return File(fileContent, contentType, fileName);
        }

        public JsonResult OnGetApplicationDetails(int applicationId)
        {
            var application = _context.Applications
                .Include(a => a.Student)
                .Include(a => a.Program)
                .Include(a => a.Documents)
                .FirstOrDefault(a => a.Id == applicationId);

            if (application == null)
            {
                return new JsonResult(new { success = false, message = "Application not found." });
            }

            var result = new
            {
                success = true,
                data = new
                {
                    application.FirstName,
                    application.LastName,
                    application.ContactEmail,
                    application.PhoneNumber,
                    application.Address,
                    application.ApplicationDate,
                    application.BirthDate,
                    application.City,
                    application.Status,
                    application.Country,
                    application.HouseNumber,
                    application.MatriculationNumber,
                    application.Nationality,
                    application.Program.Name,
                    application.Degree,
                    application.Gender,
                    application.StudyField,
                    application.UniversityName
                }
            };

            return new JsonResult(result);
        }

        public IActionResult OnPostApproveSelected(int[] selectedApplicationIds)
        {
            if (selectedApplicationIds == null || !selectedApplicationIds.Any())
            {
                TempData["ErrorMessage"] = "No applications selected for approval.";
                return RedirectToPage();
            }

            var applications = _context.Applications.Where(a => selectedApplicationIds.Contains(a.Id)).ToList();

            foreach (var application in applications)
            {
                application.Status = "Approved";
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Selected applications approved successfully.";
            return RedirectToPage();
        }

        public IActionResult OnPostRejectSelected(string selectedApplicationIds, string rejectReason)
        {
            if (string.IsNullOrWhiteSpace(selectedApplicationIds))
            {
                TempData["ErrorMessage"] = "No applications selected.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(rejectReason))
            {
                TempData["ErrorMessage"] = "Rejection reason is required.";
                return RedirectToPage();
            }

            // IDs parsen (sie kommen als CSV)
            var applicationIds = selectedApplicationIds.Split(',').Select(int.Parse).ToArray();

            // Bewerbungen abrufen
            var applications = _context.Applications.Where(a => applicationIds.Contains(a.Id)).ToList();

            if (!applications.Any())
            {
                TempData["ErrorMessage"] = "No applications found.";
                return RedirectToPage();
            }

            // Status und Grund aktualisieren
            foreach (var application in applications)
            {
                application.Status = "Rejected: " + rejectReason;
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"{applications.Count} application(s) rejected successfully!";
            return RedirectToPage();
        }

        public IActionResult OnGetGenerateReport(string reportType)
        {
            if (string.IsNullOrWhiteSpace(reportType))
            {
                TempData["ErrorMessage"] = "Please select a report type.";
                return RedirectToPage();
            }

            byte[] reportData;
            string fileName;

            switch (reportType)
            {
                case "applicants":
                    // Erstelle eine Liste aller Bewerber f�r das Auswahlverfahren
                    var applicants = _context.Applications
                        .Include(a => a.Student)
                        .Include(a => a.Program)
                        .Select(a => new
                        {
                            FirstName = a.FirstName,
                            LastName = a.LastName,
                            Program = a.Program.Name,
                            ApplicationDate = a.ApplicationDate,
                            Status = a.Status,
                            Birthdate = a.BirthDate,
                            Gender = a.Gender,
                            Nationality = a.Nationality,
                            Email = a.ContactEmail,
                            PhoneNumber = a.PhoneNumber,
                            Address = a.Address,
                            HouseNumber = a.HouseNumber,
                            City = a.City,
                            Country = a.Country,
                            UniversityName = a.UniversityName,
                            StudyField = a.StudyField,
                            Degree = a.Degree,
                            MatriculationNumber = a.MatriculationNumber
                        })
                        .ToList();

                    reportData = GenerateCsvReport(applicants);
                    fileName = "Applicants_Report.csv";
                    break;

                case "applicationsByYear":
                    // Gruppiere Bewerbungen nach Jahr
                    var applicationsByYear = _context.Applications
                        .GroupBy(a => a.ApplicationDate.Value.Year)
                        .Select(g => new
                        {
                            Year = g.Key,
                            Count = g.Count()
                        })
                        .ToList();

                    reportData = GenerateCsvReport(applicationsByYear);
                    fileName = "Applications_By_Year_Report.csv";
                    break;

                case "applicationsByHost":
                    // Gruppiere Bewerbungen nach Gastuniversit�t
                    var applicationsByHost = _context.Applications
                        .Include(a => a.Program)
                        .GroupBy(a => a.Program.Name)
                        .Select(g => new
                        {
                            HostUniversity = g.Key,
                            Count = g.Count()
                        })
                        .ToList();

                    reportData = GenerateCsvReport(applicationsByHost);
                    fileName = "Applications_By_Host_Report.csv";
                    break;

                default:
                    TempData["ErrorMessage"] = "Invalid report type selected.";
                    return RedirectToPage();
            }

            return File(reportData, "text/csv", fileName);
        }

        private byte[] GenerateCsvReport<T>(IEnumerable<T> data)
        {
            var csvBuilder = new StringBuilder();

            // Header hinzuf�gen
            var properties = typeof(T).GetProperties();
            csvBuilder.AppendLine(string.Join(";", properties.Select(p => p.Name)));

            // Datenzeilen hinzuf�gen
            foreach (var item in data)
            {
                csvBuilder.AppendLine(string.Join(";", properties.Select(p => p.GetValue(item)?.ToString()?.Replace(";", " "))));
            }

            return Encoding.UTF8.GetBytes(csvBuilder.ToString());
        }

        public IActionResult OnPostChangeEmail()
        {
            // Simulierte Organizer-Email-Adresse aus TempData abrufen
            var organizer = _context.Students.FirstOrDefault(s => !s.isStudent && s.Email == CurrentEmail);
            if (organizer == null)
            {
                TempData["ErrorMessage"] = "Current email is incorrect.";
                TempData["ActiveTab"] = "email";
                return RedirectToPage();
            }

            if (NewEmail != ConfirmEmail)
            {
                TempData["ErrorMessage"] = "New email and confirmation do not match.";
                TempData["ActiveTab"] = "email";
                return RedirectToPage();
            }

            organizer.Email = NewEmail;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Email updated successfully!";
            TempData["ActiveTab"] = "email";
            return RedirectToPage();
        }

        public IActionResult OnPostChangePassword()
        {
            // Simulierte Organizer-Email-Adresse f�r Passwort�nderung
            var organizer = _context.Students.FirstOrDefault(s => !s.isStudent);
            if (organizer == null)
            {
                TempData["ErrorMessage"] = "Invalid organizer.";
                TempData["ActiveTab"] = "password";
                return RedirectToPage();
            }

            // �berpr�fen des alten Passworts
            using (var sha256 = SHA256.Create())
            {
                var oldPasswordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(OldPassword)));
                if (organizer.PasswordHash != oldPasswordHash)
                {
                    TempData["ErrorMessage"] = "Old password is incorrect.";
                    TempData["ActiveTab"] = "password";
                    return RedirectToPage();
                }
            }

            // �berpr�fen, ob das neue Passwort mit der Best�tigung �bereinstimmt
            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match.";
                TempData["ActiveTab"] = "password";
                return RedirectToPage();
            }

            // Neues Passwort setzen
            using (var sha256 = SHA256.Create())
            {
                organizer.PasswordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(NewPassword)));
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password updated successfully!";
            TempData["ActiveTab"] = "password";
            return RedirectToPage();
        }

        public IActionResult OnPostCreateProgram()
        {
            if (string.IsNullOrWhiteSpace(ProgramName) || string.IsNullOrWhiteSpace(ProgramDescription) || ProgramDeadline == default(DateTime))
            {
                TempData["ErrorMessage"] = "All fields are required.";
                TempData["ActiveTab"] = "programs";
                return RedirectToPage();
            }

            var newProgram = new Programs
            {
                Name = ProgramName,
                Description = ProgramDescription,
                Deadline = ProgramDeadline,
                CourseOfStudy = ProgramCourseOfStudy,
                SemesterStart = ProgramSemesterStart,
                HostUniversityName = ProgramHostUniversityName
            };

            _context.Programs.Add(newProgram);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Program created successfully!";
            TempData["ActiveTab"] = "programs";
            return RedirectToPage();
        }

        public IActionResult OnPostDeletePrograms([FromForm] int[] selectedProgramIds)
        {
            if (selectedProgramIds == null || selectedProgramIds.Length == 0)
            {
                TempData["ErrorMessage"] = "No programs selected for deletion.";
                return RedirectToPage();
            }

            var programsToDelete = _context.Programs.Where(p => selectedProgramIds.Contains(p.Id)).ToList();
            _context.Programs.RemoveRange(programsToDelete);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Selected programs deleted successfully.";
            TempData["ActiveTab"] = "programs";
            return RedirectToPage();
        }

    }
}
