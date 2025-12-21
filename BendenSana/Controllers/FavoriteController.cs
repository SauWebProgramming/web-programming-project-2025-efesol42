using BendenSana.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BendenSana.Controllers
{

    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly IFavoriteRepository _favoriteRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoriteController(IFavoriteRepository favoriteRepo, UserManager<ApplicationUser> userManager)
        {
            _favoriteRepo = favoriteRepo;
            _userManager = userManager;
        }

        // FAVORİLERİM SAYFASI
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var favorites = await _favoriteRepo.GetUserFavoritesAsync(user.Id);
            return View(favorites);
        }

        // FAVORİ EKLE / ÇIKAR (TOGGLE)
        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var existingFav = await _favoriteRepo.GetFavoriteAsync(user.Id, productId);

            if (existingFav != null)
            {
                // Varsa sil
                await _favoriteRepo.RemoveAsync(existingFav);
            }
            else
            {
                // Yoksa ekle
                var newFav = new Favorite
                {
                    UserId = user.Id,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                };
                await _favoriteRepo.AddAsync(newFav);
            }

            await _favoriteRepo.SaveChangesAsync();

            return RedirectToAction("Details", "Product", new { id = productId });
        }
    }
}
