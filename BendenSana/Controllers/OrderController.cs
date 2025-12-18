using BendenSana.Models;
using BendenSana.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public OrderController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            
            var cart = _context.Set<Cart>()
                               .Include(c => c.Items)
                               .ThenInclude(i => i.Product)
                               .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Sepetiniz boş.";
                return RedirectToAction("Index", "Cart");
            }

            var addresses = await _context.Set<Address>()
                                          .Where(a => a.UserId == user.Id)
                                          .ToListAsync();

            var model = new CheckoutViewModel
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.Items.Sum(x => x.Quantity * x.Product.Price),
                UserAddresses = addresses,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

           
            var cart = _context.Set<Cart>()
                               .Include(c => c.Items)
                               .ThenInclude(i => i.Product) 
                               .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Cart");

            
            if (model.SelectedAddressId == 0)
            {
                ModelState.AddModelError("", "Lütfen bir teslimat adresi seçiniz.");
                
                model.UserAddresses = await _context.Set<Address>().Where(a => a.UserId == user.Id).ToListAsync();
                model.CartItems = cart.Items.ToList();
                model.TotalAmount = cart.Items.Sum(x => x.Quantity * x.Product.Price);
                return View(model);
            }

            decimal subtotal = cart.Items.Sum(x => x.Quantity * x.Product.Price);
            decimal shippingCost = 0;
            decimal discount = 0;
            decimal total = subtotal + shippingCost - discount;

            var order = new Order
            {
                BuyerId = user.Id,
                AddressId = model.SelectedAddressId,
                Status = OrderStatus.preparing,
                Subtotal = subtotal,
                ShippingCost = shippingCost,
                DiscountAmount = discount,
                TotalPrice = total,
                OrderCode = "ORD-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<Order>().Add(order);
            _context.SaveChanges(); 

           
            foreach (var cartItem in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price,
                   
                    SellerId = cartItem.Product.SellerId
                };
                _context.Set<OrderItem>().Add(orderItem);
            }

           
            _context.Set<CartItem>().RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Siparişiniz başarıyla alındı!";
            return RedirectToAction("Index"); 
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _context.Set<Order>()
                                       .Include(o => o.Items).ThenInclude(oi => oi.Product)
                                       .Where(o => o.BuyerId == user.Id)
                                       .OrderByDescending(o => o.CreatedAt)
                                       .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Set<Order>()
                                      .Include(o => o.Items).ThenInclude(i => i.Product)
                                      .Include(o => o.Address)
                                      .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }
    }
}