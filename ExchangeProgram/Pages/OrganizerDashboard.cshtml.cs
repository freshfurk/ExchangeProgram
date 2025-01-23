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

        public List<Programs> Programs { get; set; } = new List<Programs>();

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

        public IActionResult OnGet()
        {
            // Studenten mit ihren Dokumenten laden
            Students = _context.Students
                .Where(s => s.isStudent)
                .Include(s => s.Documents) // Dokumente laden
                .ToList();

            Programs = _context.Programs.ToList();

            return Page();
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
            // Simulierte Organizer-Email-Adresse für Passwortänderung
            var organizer = _context.Students.FirstOrDefault(s => !s.isStudent);
            if (organizer == null)
            {
                TempData["ErrorMessage"] = "Invalid organizer.";
                TempData["ActiveTab"] = "password";
                return RedirectToPage();
            }

            // Überprüfen des alten Passworts
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

            // Überprüfen, ob das neue Passwort mit der Bestätigung übereinstimmt
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

        //public IActionResult OnPostDeleteStudent(int studentId)
        //{
        //    var student = _context.Students.FirstOrDefault(s => s.Id == studentId && s.isStudent);
        //    if (student != null)
        //    {
        //        // Löschen der zugehörigen Dokumente
        //        var documents = _context.Documents.Where(d => d.StudentId == studentId).ToList();
        //        _context.Documents.RemoveRange(documents);

        //        // Löschen des Studenten
        //        _context.Students.Remove(student);
        //        _context.SaveChanges();

        //        TempData["SuccessMessage"] = "Student and associated documents deleted successfully!";
        //    }
        //    else
        //    {
        //        TempData["ErrorMessage"] = "Student not found.";
        //    }

        //    TempData["ActiveTab"] = "students";
        //    return RedirectToPage();
        //}

        //public IActionResult OnGetDownloadDocument(int documentId)
        //{
        //    var document = _context.Documents.FirstOrDefault(d => d.Id == documentId);
        //    if (document != null)
        //    {
        //        return File(document.FileData, "application/octet-stream", document.FileName);
        //    }

        //    TempData["ErrorMessage"] = "Document not found.";
        //    TempData["ActiveTab"] = "students";
        //    return RedirectToPage();
        //}

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
                Deadline = ProgramDeadline
            };

            _context.Programs.Add(newProgram);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Program created successfully!";
            TempData["ActiveTab"] = "programs";
            return RedirectToPage();
        }

        public IActionResult OnPostDeleteProgram(int programId)
        {
            var program = _context.Programs.FirstOrDefault(p => p.Id == programId);
            if (program != null)
            {
                _context.Programs.Remove(program);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Program deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Program not found.";
            }

            TempData["ActiveTab"] = "programs";
            return RedirectToPage();
        }

    }
}
