using BendenSana.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using BendenSana.Models;

namespace BendenSana.Controllers
{
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly AppDbContext _context; 

        public CartController(UserManager<ApplicationUser> userManager,
                              IProductRepository productRepository,
                              IRepository<Cart> cartRepository,
                              IRepository<CartItem> cartItemRepository,
                              AppDbContext context)
        {
            _userManager = userManager;
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _context = context;
        }

        // SEPETİM SAYFASI
        public IActionResult Index()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Index", "Home");

            
            var cart = _context.Set<Cart>()
                               .Include(c => c.Items)
                               .ThenInclude(ci => ci.Product)
                               .ThenInclude(p => p.Images)
                               .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null)
            {
                return View(new List<CartItem>());
            }

            
            return View(cart.Items);
        }

        // SEPETE EKLEME İŞLEMİ
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Index", "Home");

            
            var cart = _context.Set<Cart>()
                               .Include(c => c.Items)
                               .FirstOrDefault(c => c.UserId == user.Id);

            // Sepet yoksa oluştur
            if (cart == null)
            {
                cart = new Cart { UserId = user.Id, CreatedAt = DateTime.UtcNow };
                _context.Set<Cart>().Add(cart);
                _context.SaveChanges();
            }

            
            var cartItem = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (cartItem != null)
            {
                
                cartItem.Quantity += quantity;
            }
            else
            {
                
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.Set<CartItem>().Add(cartItem);
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        private ApplicationUser GetCurrentUser()
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                return _userManager.FindByIdAsync(userId).Result;
            }
            return _userManager.Users.FirstOrDefault();
        }

        // SEPETTEN SİLME İŞLEMİ
        public IActionResult RemoveFromCart(int id)
        {
            var cartItem = _context.Set<global::CartItem>().Find(id);

            if (cartItem != null)
            {
                _context.Set<global::CartItem>().Remove(cartItem);
                _context.SaveChanges();
            }

            
            return RedirectToAction("Index");
        }
    }
}