using BendenSana.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BendenSana.Controllers
{

    [Authorize(Roles = "User")]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartRepository _cartRepo;

        public CartController(UserManager<ApplicationUser> userManager, ICartRepository cartRepo)
        {
            _userManager = userManager;
            _cartRepo = cartRepo;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = await _cartRepo.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                return View(new List<CartItem>());
            }

            return View(cart.Items);
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = await _cartRepo.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                cart = await _cartRepo.CreateCartAsync(userId);
            }

            var cartItem = await _cartRepo.GetCartItemAsync(cart.Id, productId);

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
                await _cartRepo.AddCartItemAsync(cartItem);
            }

            await _cartRepo.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var cartItem = await _cartRepo.GetCartItemByIdAsync(id);

            if (cartItem != null)
            {
                await _cartRepo.RemoveCartItemAsync(cartItem);
                await _cartRepo.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var cartItem = await _cartRepo.GetCartItemByIdAsync(id);

            if (cartItem != null)
            {
                if (quantity < 1) quantity = 1;
                if (quantity > 10) quantity = 10;

                cartItem.Quantity = quantity;
                await _cartRepo.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
