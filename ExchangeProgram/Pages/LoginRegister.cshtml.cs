using ExchangeProgram.Data;
using ExchangeProgram.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;

namespace ExchangeProgram.Pages
{
    public class LoginRegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginRegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Student Student { get; set; }

        [BindProperty]
        public string LoginEmail { get; set; }

        [BindProperty]
        public string LoginPassword { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public IActionResult OnPostRegister()
        {
            //if (!ModelState.IsValid) return Page();

            // Prüfen, ob die Passwörter übereinstimmen
            if (Password != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                return RedirectToPage();
            }

            // Passwort verschlüsseln
            using (var sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(Password);
                Student.PasswordHash = Convert.ToBase64String(sha256.ComputeHash(passwordBytes));
            }

            // Setze isStudent immer auf true
            Student.isStudent = true;

            _context.Students.Add(Student);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToPage();
        }

        public IActionResult OnPostLogin()
        {
            var user = _context.Students.FirstOrDefault(s => s.Email == LoginEmail);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage("/LoginRegister");
            }

            using (var sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(LoginPassword);
                var hashedPassword = Convert.ToBase64String(sha256.ComputeHash(passwordBytes));

                if (user.PasswordHash != hashedPassword)
                {
                    TempData["ErrorMessage"] = "Invalid password.";
                    return RedirectToPage("/LoginRegister");
                }
            }

            // Benutzer-ID speichern und weiterleiten
            // Weiterleitung basierend auf der Rolle
            if (user.isStudent)
            {
                return RedirectToPage("/Index", new { id = user.Id });
            }
            else
            {
                return RedirectToPage("/OrganizerDashboard", new { id = user.Id });
            }
        }
    }
}
