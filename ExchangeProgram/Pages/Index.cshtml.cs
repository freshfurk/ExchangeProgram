using ExchangeProgram.Data;
using ExchangeProgram.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;

namespace ExchangeProgram.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
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

        public IActionResult OnPostRegister()
        {
            //if (!ModelState.IsValid) return Page();

            // Passwort verschlüsseln
            using (var sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(Password);
                Student.PasswordHash = Convert.ToBase64String(sha256.ComputeHash(passwordBytes));
            }

            _context.Students.Add(Student);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToPage();
        }

        public IActionResult OnPostLogin()
        {
            if (string.IsNullOrEmpty(LoginEmail) || string.IsNullOrEmpty(LoginPassword))
            {
                TempData["ErrorMessage"] = "Invalid login credentials.";
                return RedirectToPage();
            }

            // Prüfen, ob der Benutzer existiert
            var user = _context.Students.FirstOrDefault(s => s.Email == LoginEmail);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            // Passwort prüfen
            using (var sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(LoginPassword);
                var hashedPassword = Convert.ToBase64String(sha256.ComputeHash(passwordBytes));

                if (user.PasswordHash != hashedPassword)
                {
                    TempData["ErrorMessage"] = "Invalid password.";
                    return RedirectToPage();
                }
            }

            TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}!";
            // Vorname speichern und weiterleiten
            TempData["FirstName"] = user.FirstName;
            return RedirectToPage("/Dashboard");
        }
    }
}

