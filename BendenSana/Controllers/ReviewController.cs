using BendenSana.Models;
using BendenSana.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IProductRepository _productRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(
            IReviewRepository reviewRepo,
            IProductRepository productRepo,
            UserManager<ApplicationUser> userManager)
        {
            _reviewRepo = reviewRepo;
            _productRepo = productRepo;
            _userManager = userManager;
        }

        // YORUM OLUŞTURMA
        [HttpPost]
        public async Task<IActionResult> Create(int productId, string comment, int rating)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null) return NotFound("Ürün bulunamadı.");

            // Kendi ürününe yorum yapma kontrolü
            if (product.SellerId == user.Id)
            {
                TempData["Error"] = "Kendi ilanınıza yorum yapamazsınız.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            var review = new Review
            {
                UserId = user.Id,
                ProductId = productId,
                Comment = comment,
                Rating = rating,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepo.AddAsync(review);
            await _reviewRepo.SaveChangesAsync();

            TempData["Success"] = "Yorumunuz eklendi!";
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        // YORUM SİLME
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewRepo.GetByIdAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (review != null && (review.UserId == user.Id || User.IsInRole("Admin")))
            {
                await _reviewRepo.DeleteAsync(review);
                await _reviewRepo.SaveChangesAsync();
                TempData["Success"] = "Yorum silindi.";
            }

            // Kullanıcıyı geldiği sayfaya geri gönder
            var referer = Request.Headers["Referer"].ToString();
            return string.IsNullOrEmpty(referer) ? RedirectToAction("Index", "Home") : Redirect(referer);
        }
    }
}