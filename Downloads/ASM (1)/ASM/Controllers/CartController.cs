using Microsoft.AspNetCore.Mvc;
using ASM.Models;
using ASM.Data;
using Newtonsoft.Json;

namespace ASM.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var item = _context.FoodItems.Find(id);
            if (item != null)
            {
                var cart = GetCart();
                var cartItem = cart.FirstOrDefault(c => c.FoodItemId == id);
                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    cart.Add(new OrderDetail 
                    { 
                        FoodItemId = id, 
                        FoodItem = item, 
                        Quantity = quantity, 
                        Price = item.Price 
                    });
                }
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.FoodItemId == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }
        
        // Checkout
        [HttpGet]
        public IActionResult Checkout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout") });
            }
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index");
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> CheckoutConfirm(string address)
        {
             if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");
             
             var cart = GetCart();
             if (cart.Count == 0) return RedirectToAction("Index");

             var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
             
             var order = new Order
             {
                 UserId = user.Id,
                 OrderDate = DateTime.Now,
                 Status = OrderStatus.Pending,
                 TotalAmount = cart.Sum(c => c.Quantity * c.Price)
             };
             
             _context.Orders.Add(order);
             await _context.SaveChangesAsync();

             foreach (var item in cart)
             {
                 var detail = new OrderDetail
                 {
                     OrderId = order.Id,
                     FoodItemId = item.FoodItemId,
                     Quantity = item.Quantity,
                     Price = item.Price
                 };
                 _context.OrderDetails.Add(detail);
             }
             await _context.SaveChangesAsync();

             // Clear Cart
             HttpContext.Session.Remove("Cart");

             return RedirectToAction("History", "Order");
        }

        private List<OrderDetail> GetCart()
        {
            var sessionCart = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionCart))
            {
                return new List<OrderDetail>();
            }
            return JsonConvert.DeserializeObject<List<OrderDetail>>(sessionCart);
        }

        private void SaveCart(List<OrderDetail> cart)
        {
            var json = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", json);
        }
    }
}
