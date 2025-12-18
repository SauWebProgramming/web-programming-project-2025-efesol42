using BendenSana.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        [HttpPost]
        public async Task<IActionResult> Create(int productId, string comment, int rating)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            
            var product = await _context.Set<Product>().FindAsync(productId);
            if (product == null) return NotFound("Ürün bulunamadı.");

            
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

            
            _context.Set<Review>().Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yorumunuz eklendi!";

            
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Set<Review>().FindAsync(id);
            var user = await _userManager.GetUserAsync(User);

         
            if (review != null && (review.UserId == user.Id || User.IsInRole("Admin")))
            {
                _context.Set<Review>().Remove(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Yorum silindi.";
            }

         
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}