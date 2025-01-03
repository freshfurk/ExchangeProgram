using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExchangeProgram.Pages
{
    public class DashboardModel : PageModel
    {
        public string FirstName { get; set; }

        public void OnGet()
        {
            // Beispiel: Laden des Vornamens aus TempData (oder Session)
            if (TempData["FirstName"] != null)
            {
                FirstName = TempData["FirstName"].ToString();
            }
        }
    }
}
