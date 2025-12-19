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

        // ==========================================
        // 1. CHECKOUT SAYFASI (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // A. Sepeti Getir
            var cart = await _context.Set<Cart>()
                                     .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
                                     .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Sepetiniz boş.";
                return RedirectToAction("Index", "Cart");
            }

            // B. Kayıtlı Adresleri Getir
            var addresses = await _context.Set<Address>().Where(a => a.UserId == user.Id).ToListAsync();
            var primaryAddress = addresses.FirstOrDefault();

            // C. Kayıtlı Kredi Kartını Getir (Otomatik Doldurma İçin)
            var savedCard = await _context.Set<UserCard>().FirstOrDefaultAsync(c => c.UserId == user.Id);

            // D. Modeli Hazırla
            var model = new CheckoutViewModel
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.Items.Sum(x => x.Quantity * x.Product.Price),

                // Adres Listesi (Dropdown için)
                UserAddresses = addresses,
                SelectedAddressId = primaryAddress?.Id ?? 0,

                // Fatura Formu (Varsayılan adresten doldur)
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? "",
                AddressLine = primaryAddress?.AddressLine ?? "",
                City = primaryAddress?.City ?? "",
                ZipCode = primaryAddress?.ZipCode ?? "",

                // Ödeme Formu (Kayıtlı karttan doldur)
                CardHolderName = savedCard?.CardHolderName ?? $"{user.FirstName} {user.LastName}",
                CardNumber = savedCard?.CardNumber ?? "",
                ExpiryDate = savedCard?.ExpiryDate ?? "",
                Cvv = savedCard?.Cvv ?? ""
            };

            return View(model);
        }

        // ==========================================
        // 2. SİPARİŞİ TAMAMLA (POST)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // A. Sepeti Tekrar Çek (Güvenlik ve Tutar için)
            var cart = await _context.Set<Cart>()
                                     .Include(c => c.Items).ThenInclude(i => i.Product)
                                     .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Cart");

            // B. Model Validasyonu Başarısızsa View'ı Tekrar Doldur
            model.CartItems = cart.Items.ToList();
            model.TotalAmount = cart.Items.Sum(x => x.Quantity * x.Product.Price);
            if (model.UserAddresses == null || !model.UserAddresses.Any())
            {
                model.UserAddresses = await _context.Set<Address>().Where(a => a.UserId == user.Id).ToListAsync();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // C. ADRES MANTIĞI (Mevcut mu Yeni mi?)
            int? finalAddressId = null;

            if (model.SelectedAddressId != 0)
            {
                // Kullanıcı listeden bir adres seçti
                finalAddressId = model.SelectedAddressId;
            }
            else
            {
                // Kullanıcı "Yeni Adres" girdi -> Önce Adresi Kaydet
                var newAddress = new Address
                {
                    UserId = user.Id,
                    Title = "Sipariş Adresi",
                    AddressLine = model.AddressLine,
                    City = model.City,
                    ZipCode = model.ZipCode,
                    Country = "Türkiye"
                };

                _context.Set<Address>().Add(newAddress);
                await _context.SaveChangesAsync(); // ID oluşması için save şart
                finalAddressId = newAddress.Id;
            }

            // D. SİPARİŞİ OLUŞTUR (Order Tablosu)
            var order = new Order
            {
                BuyerId = user.Id,
                AddressId = finalAddressId, // Belirlenen Adres ID'si
                Status = OrderStatus.preparing, // Enum
                Subtotal = model.TotalAmount,
                ShippingCost = 0,
                DiscountAmount = 0,
                TotalPrice = model.TotalAmount,
                OrderCode = "ORD-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<Order>().Add(order);
            await _context.SaveChangesAsync(); // Order ID oluşması için save şart

            // E. SİPARİŞ KALEMLERİNİ OLUŞTUR (OrderItems Tablosu)
            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price,
                    SellerId = item.Product.SellerId
                };
                _context.Set<OrderItem>().Add(orderItem);
            }

            // F. SEPETİ TEMİZLE
            _context.Set<CartItem>().RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Siparişiniz başarıyla alındı!";
            return RedirectToAction("Index"); // Sipariş Geçmişine Yönlendir
        }

        // ==========================================
        // 3. SİPARİŞ GEÇMİŞİ (INDEX)
        // ==========================================
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

        // ==========================================
        // 4. SİPARİŞ DETAYI (DETAILS)
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Set<Order>()
                                      .Include(o => o.Items)
                                          .ThenInclude(oi => oi.Product)
                                          .ThenInclude(p => p.Images)
                                      .Include(o => o.Address) // Adres bilgisi için
                                      .FirstOrDefaultAsync(o => o.Id == id && o.BuyerId == user.Id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}