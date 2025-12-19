using BendenSana.Models; // ApplicationUser burada
using BendenSana.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public PaymentController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Ödeme Bilgilerini Göster
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Kullanıcının kayıtlı kartını bul (Varsayılan veya ilk kart)
            var card = await _context.Set<UserCard>().FirstOrDefaultAsync(c => c.UserId == user.Id);

            var model = new PaymentOptionsViewModel
            {
                // Eğer kart yoksa isim soyisimi user'dan alabiliriz
                FirstName = card != null ? card.CardHolderName.Split(' ')[0] : user.FirstName,
                LastName = card != null && card.CardHolderName.Contains(' ') ? card.CardHolderName.Split(' ')[1] : user.LastName,
                CardNumber = card?.CardNumber,
                ExpiryDate = card?.ExpiryDate,
                Cvv = card?.Cvv
            };

            return View(model);
        }

        // POST: Ödeme Bilgilerini Kaydet/Güncelle
        [HttpPost]
        public async Task<IActionResult> Index(PaymentOptionsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var card = await _context.Set<UserCard>().FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (card == null)
            {
                // Yeni Kart Ekle
                card = new UserCard
                {
                    UserId = user.Id,
                    CardHolderName = $"{model.FirstName} {model.LastName}",
                    CardNumber = model.CardNumber ?? "",
                    ExpiryDate = model.ExpiryDate,
                    Cvv = model.Cvv,
                    IsDefault = true
                };
                _context.Set<UserCard>().Add(card);
            }
            else
            {
                // Mevcut Kartı Güncelle
                card.CardHolderName = $"{model.FirstName} {model.LastName}";
                card.CardNumber = model.CardNumber ?? "";
                card.ExpiryDate = model.ExpiryDate;
                card.Cvv = model.Cvv;

                _context.Set<UserCard>().Update(card);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Ödeme seçenekleriniz güncellendi.";

            return RedirectToAction("Index");
        }
    }
}