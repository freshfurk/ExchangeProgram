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

        public List<Programs> Programs { get; set; }
        public bool? IsStudent { get; set; }

        public void OnGet()
        {
            // Nur Programme mit gültiger Deadline laden
            Programs = _context.Programs
                .Where(p => p.Deadline > DateTime.Now) // Programme filtern, deren Deadline noch nicht überschritten ist
                .ToList();

            if (Request.Query.ContainsKey("id") && int.TryParse(Request.Query["id"], out var userId))
            {
                var user = _context.Students.FirstOrDefault(s => s.Id == userId);
                if (user != null)
                {
                    IsStudent = user.isStudent;
                }
            }
        }

        public IActionResult OnGetApply(int? id)
        {
            // Prüfen, ob eine ID in der URL vorhanden ist (Prüft auf Login-Status)
            if (!id.HasValue)
            {
                // Benutzer ist nicht eingeloggt, Weiterleitung zur Login-Seite
                //TempData["ErrorMessage"] = "You must be logged in to apply.";
                return RedirectToPage("/LoginRegister");
            }

            return RedirectToPage("/Apply", new { id });
        }
    }
}
