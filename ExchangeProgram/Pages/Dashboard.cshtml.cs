using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExchangeProgram.Models;
using ExchangeProgram.Data;

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

            TempData["UserId"] = id; // Id in TempData speichern
            return Page();
        }

        public IActionResult OnPostSaveProfile()
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

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Profile updated successfully!";

            return RedirectToPage("/Dashboard", new { id });
        }
    }
}
