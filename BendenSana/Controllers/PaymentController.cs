using BendenSana.Models; // ApplicationUser burada
using BendenSana.Models.Repositories;
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
        private readonly IPaymentRepository _paymentRepo;

        public PaymentController(UserManager<ApplicationUser> userManager, IPaymentRepository paymentRepo)
        {
            _userManager = userManager;
            _paymentRepo = paymentRepo;
        }

        // GET: Ödeme Bilgilerini Göster
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Kullanıcının kayıtlı kartını repository üzerinden bul
            var card = await _paymentRepo.GetCardByUserIdAsync(user.Id);

            var model = new PaymentOptionsViewModel
            {
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

            var card = await _paymentRepo.GetCardByUserIdAsync(user.Id);

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
                await _paymentRepo.AddCardAsync(card);
            }
            else
            {
                // Mevcut Kartı Güncelle
                card.CardHolderName = $"{model.FirstName} {model.LastName}";
                card.CardNumber = model.CardNumber ?? "";
                card.ExpiryDate = model.ExpiryDate;
                card.Cvv = model.Cvv;

                await _paymentRepo.UpdateCardAsync(card);
            }

            await _paymentRepo.SaveChangesAsync();
            TempData["Success"] = "Ödeme seçenekleriniz güncellendi.";

            return RedirectToAction("Index");
        }
    }
}