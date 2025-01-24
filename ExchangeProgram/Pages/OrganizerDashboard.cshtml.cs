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


        public JsonResult OnPostApproveApplications([FromBody] int[] applicationIds)
        {
            var applications = _context.Applications.Where(a => applicationIds.Contains(a.Id)).ToList();

            foreach (var application in applications)
            {
                application.Status = "Accepted";
            }

            _context.SaveChanges();
            return new JsonResult(new { success = true, message = "Selected applications approved." });
        }

        public JsonResult OnPostRejectApplications([FromBody] dynamic payload)
        {
            int[] applicationIds = payload.applicationIds.ToObject<int[]>();
            string reason = payload.reason;

            var applications = _context.Applications.Where(a => applicationIds.Contains(a.Id)).ToList();

            foreach (var application in applications)
            {
                application.Status = "Rejected - " + reason;
            }

            _context.SaveChanges();
            return new JsonResult(new { success = true, message = "Selected applications rejected." });
        }


        public IActionResult OnPostUpdateStatus()
        {
            if (SelectedApplicationIds == null || !SelectedApplicationIds.Any())
            {
                TempData["ErrorMessage"] = "No applications selected.";
                return RedirectToPage();
            }

            var applications = _context.Applications.Where(a => SelectedApplicationIds.Contains(a.Id)).ToList();

            foreach (var application in applications)
            {
                application.Status = StatusUpdate;
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Status updated successfully.";
            return RedirectToPage();
        }

        public JsonResult OnPostGenerateReport(string reportType)
        {
            object reportData;

            switch (reportType)
            {
                case "Applicants":
                    reportData = _context.Applications
                        .Where(a => a.Status == "Applicant")
                        .Include(a => a.Student)
                        .Include(a => a.Program)
                        .Select(a => new
                        {
                            a.Id,
                            a.FirstName,
                            a.LastName,
                            a.Program.Name
                        }).ToList();
                    break;

                case "ApplicationsPerYear":
                    reportData = _context.Applications
                        .GroupBy(a => a.ApplicationDate)
                        .Select(g => new { Year = g.Key, Count = g.Count() })
                        .ToList();
                    break;

                case "ApplicationsPerHostUniversity":
                    reportData = _context.Applications
                        .GroupBy(a => a.Program.Name)
                        .Select(g => new { Program = g.Key, Count = g.Count() })
                        .ToList();
                    break;

                default:
                    return new JsonResult(new { success = false, message = "Invalid report type." });
            }

            return new JsonResult(new { success = true, data = reportData });
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
