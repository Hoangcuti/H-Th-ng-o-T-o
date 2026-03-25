using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASM.Data;
using ASM.Models;

namespace ASM.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Menu
        public async Task<IActionResult> Index(string category, string searchStr)
        {
            ViewData["CurrentCategory"] = category;
            ViewData["CurrentFilter"] = searchStr;

            var items = from s in _context.FoodItems
                        select s;

            if (!String.IsNullOrEmpty(searchStr))
            {
                items = items.Where(s => s.Name.Contains(searchStr) || s.Description.Contains(searchStr));
            }

            if (!String.IsNullOrEmpty(category))
            {
                if (category == "Combo")
                {
                    // Handle combos separately or mix them?
                    // For simplicity, let's just return View("ComboIndex", _context.Combos.ToList()) or similar if requested
                    // But if we want unified list, we need a ViewModel.
                    // Let's assume for now we list FoodItems that match category.
                    items = items.Where(s => s.Category == category);
                }
                else 
                {
                     items = items.Where(s => s.Category == category);
                }
            }

            return View(await items.ToListAsync());
        }

        // GET: Menu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }
    }
}
