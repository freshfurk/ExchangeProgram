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

        public void OnGet()
        {
            Programs = _context.Programs.ToList();
        }
    }
}

