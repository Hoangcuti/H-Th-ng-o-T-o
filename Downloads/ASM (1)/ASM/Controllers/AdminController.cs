using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASM.Data;
using ASM.Models;

namespace ASM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // === USER MANAGEMENT ===
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // === FOOD MANAGEMENT ===
        public async Task<IActionResult> FoodItems()
        {
            return View(await _context.FoodItems.ToListAsync());
        }

        public IActionResult CreateFood()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateFood(FoodItem foodItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(foodItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(FoodItems));
            }
            return View(foodItem);
        }

        public async Task<IActionResult> EditFood(int? id)
        {
            if (id == null) return NotFound();
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null) return NotFound();
            return View(foodItem);
        }

        [HttpPost]
        public async Task<IActionResult> EditFood(int id, FoodItem foodItem)
        {
            if (id != foodItem.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(foodItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.FoodItems.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(FoodItems));
            }
            return View(foodItem);
        }

        public async Task<IActionResult> DeleteFood(int? id)
        {
            if (id == null) return NotFound();
            var foodItem = await _context.FoodItems.FirstOrDefaultAsync(m => m.Id == id);
            if (foodItem == null) return NotFound();
            return View(foodItem);
        }

        [HttpPost, ActionName("DeleteFood")]
        public async Task<IActionResult> DeleteFoodConfirmed(int id)
        {
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem != null)
            {
                _context.FoodItems.Remove(foodItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(FoodItems));
        }

        // === ORDER MANAGEMENT ===
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders.Include(o => o.User).OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Orders));
        }

        // === COMBO MANAGEMENT ===
        public async Task<IActionResult> Combos()
        {
            return View(await _context.Combos.ToListAsync());
        }

        public IActionResult CreateCombo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCombo(Combo combo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(combo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Combos));
            }
            return View(combo);
        }

        public async Task<IActionResult> EditCombo(int? id)
        {
            if (id == null) return NotFound();
            var combo = await _context.Combos.FindAsync(id);
            if (combo == null) return NotFound();
            return View(combo);
        }

        [HttpPost]
        public async Task<IActionResult> EditCombo(int id, Combo combo)
        {
            if (id != combo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(combo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Combos.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Combos));
            }
            return View(combo);
        }

        public async Task<IActionResult> DeleteCombo(int? id)
        {
            if (id == null) return NotFound();
            var combo = await _context.Combos.FirstOrDefaultAsync(m => m.Id == id);
            if (combo == null) return NotFound();
            return View(combo);
        }

        [HttpPost, ActionName("DeleteCombo")]
        public async Task<IActionResult> DeleteComboConfirmed(int id)
        {
            var combo = await _context.Combos.FindAsync(id);
            if (combo != null)
            {
                _context.Combos.Remove(combo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Combos));
        }

    }
}
