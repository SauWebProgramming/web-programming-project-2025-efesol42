using BendenSana.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoriteController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

       
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var favorites = await _context.Favorites
                                          .Include(f => f.Product)
                                            .ThenInclude(p => p.Images)
                                          .Where(f => f.UserId == user.Id)
                                          .ToListAsync();
            return View(favorites);
        }

        
        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

           
            var existingFav = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ProductId == productId);

            if (existingFav != null)
            {
                
                _context.Favorites.Remove(existingFav);
            }
            else
            {
                var newFav = new Favorite
                {
                    UserId = user.Id,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Favorites.Add(newFav);
            }

            await _context.SaveChangesAsync();

            
            return RedirectToAction("Details", "Product", new { id = productId });
        }
    }
}