using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExchangeProgram.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            TempData.Clear();
            return RedirectToPage("/Index");
        }
    }
}
