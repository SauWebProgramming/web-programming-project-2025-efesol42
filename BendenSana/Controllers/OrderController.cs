using BendenSana.Models;
using BendenSana.Models.Repositories;
using BendenSana.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    [Authorize(Roles ="User")]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderRepository orderRepo, UserManager<ApplicationUser> userManager)
        {
            _orderRepo = orderRepo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var cart = await _orderRepo.GetCartWithItemsAsync(user.Id);
            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Cart");

            var addresses = await _orderRepo.GetUserAddressesAsync(user.Id);
            var savedCard = await _orderRepo.GetUserSavedCardAsync(user.Id);

            var model = new CheckoutViewModel
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.Items.Sum(x => x.Quantity * x.Product.Price),

                // Adres Listesi (Dropdown için)
                UserAddresses = addresses,
                SelectedAddressId = addresses.FirstOrDefault()?.Id ?? 0,

                // Fatura Formu (Varsayılan adresten doldur)
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? "",
                AddressLine = addresses.FirstOrDefault()?.AddressLine ?? "",
                City = addresses.FirstOrDefault()?.City ?? "",
                ZipCode = addresses.FirstOrDefault()?.ZipCode ?? "",

                // Ödeme Formu (Kayıtlı karttan doldur)
                CardHolderName = savedCard?.CardHolderName ?? $"{user.FirstName} {user.LastName}",
                CardNumber = savedCard?.CardNumber ?? "",
                ExpiryDate = savedCard?.ExpiryDate ?? "",
                Cvv = savedCard?.Cvv ?? ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _orderRepo.GetCartWithItemsAsync(user.Id);

            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Cart");

            if (!ModelState.IsValid)
            {
                model.CartItems = cart.Items.ToList();
                model.UserAddresses = await _orderRepo.GetUserAddressesAsync(user.Id);
                return View(model);
            }

            // Adres Belirleme
            int finalAddressId = model.SelectedAddressId;
            if (finalAddressId == 0)
            {
                var newAddress = new Address
                {
                    UserId = user.Id,
                    Title = "Sipariş Adresi",
                    AddressLine = model.AddressLine,
                    City = model.City,
                    ZipCode = model.ZipCode,
                    Country = "Türkiye"
                };
                finalAddressId = await _orderRepo.CreateAddressAsync(newAddress);
            }

            // Sipariş Hazırlama
            var order = new Order
            {
                BuyerId = user.Id,
                AddressId = finalAddressId,
                Status = OrderStatus.preparing,
                TotalPrice = cart.Items.Sum(x => x.Quantity * x.Product.Price),
                OrderCode = "ORD-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                CreatedAt = DateTime.UtcNow
            };

            var orderItems = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Product.Price,
                SellerId = item.Product.SellerId
            }).ToList();

            // Veritabanına Yazma
            await _orderRepo.CreateOrderAsync(order, orderItems);
            await _orderRepo.ClearCartAsync(cart.Id);
            await _orderRepo.SaveChangesAsync();

            TempData["Success"] = "Siparişiniz başarıyla alındı!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _orderRepo.GetUserOrdersAsync(user.Id);
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _orderRepo.GetOrderDetailsAsync(id, user.Id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}