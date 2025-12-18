using BendenSana.Models;    
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;



namespace BendenSana.Controllers
{
    [Authorize]
    public class TradeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TradeController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Create(int targetProductId)
        {
            var user = await _userManager.GetUserAsync(User);


            var targetProduct = await _context.Set<Product>()
                                              .Include(p => p.Seller)
                                              .FirstOrDefaultAsync(p => p.Id == targetProductId);

            if (targetProduct == null) return NotFound("Ürün bulunamadı.");


            if (targetProduct.SellerId == user.Id)
            {
                TempData["Error"] = "Kendi ürününüze takas teklif edemezsiniz.";
                return RedirectToAction("Details", "Product", new { id = targetProductId });
            }


            ViewBag.TargetProductName = targetProduct.Title;
            ViewBag.TargetProductId = targetProductId;
            ViewBag.SellerName = targetProduct.Seller.UserName;


            var myProducts = await _context.Set<Product>()
                                           .Where(p => p.SellerId == user.Id)
                                           .OrderByDescending(p => p.CreatedAt)
                                           .ToListAsync();

            ViewBag.MyProducts = new SelectList(myProducts, "Id", "Title");

            return View();
        }


        [HttpPost]

        public async Task<IActionResult> Create(int targetProductId, int? offeredProductId, decimal? offeredCash, string message)
        {
            var user = await _userManager.GetUserAsync(User);
            var targetProduct = await _context.Set<Product>().FindAsync(targetProductId);

            if (targetProduct == null) return NotFound();


            if (targetProduct.SellerId == user.Id) return RedirectToAction("Details", "Product", new { id = targetProductId });


            if (offeredProductId == null && (offeredCash == null || offeredCash == 0))
            {
                TempData["Error"] = "Lütfen takas için bir ürün ya da üstüne nakit para teklif edin.";
                return RedirectToAction("Details", "Product", new { id = targetProductId });
            }

            var offer = new TradeOffer
            {
                OffererId = user.Id,
                ReceiverId = targetProduct.SellerId,
                Status = TradeOfferStatus.Pending,
                OffererMessage = message,
                OfferedCashAmount = offeredCash, 
                CreatedAt = DateTime.UtcNow,
                
            };

            
            var targetItem = new TradeItem
            {
                ProductId = targetProductId,
            
                ItemType = TradeItemType.requested 
            };
            offer.Items.Add(targetItem);

            
            if (offeredProductId.HasValue)
            {
                var offeredItem = new TradeItem
                {
                    ProductId = offeredProductId.Value,
            
                    ItemType = TradeItemType.offered 
                };
                offer.Items.Add(offeredItem);
            }

            _context.TradeOffers.Add(offer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Takas teklifi başarıyla gönderildi!";
            return RedirectToAction("Details", "Product", new { id = targetProductId });
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var trades = await _context.TradeOffers
                .Include(t => t.Offerer) 
                .Include(t => t.Receiver)
                .Include(t => t.Items)  
                    .ThenInclude(i => i.Product) 
                .Where(t => t.OffererId == userId || t.ReceiverId == userId) 
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(trades);
        }

        
        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            var userId = _userManager.GetUserId(User);

        
            var offer = await _context.TradeOffers
                .Include(t => t.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (offer == null) return NotFound();

        
            if (offer.ReceiverId != userId) return Forbid();

            
            offer.Status = TradeOfferStatus.Accepted;
            
            foreach (var item in offer.Items)
            {
                if (item.Product != null)
                {
            
                    item.Product.Status = ProductStatus.sold;

                   
                }
            }



            await _context.SaveChangesAsync();
            TempData["Success"] = "Teklif kabul edildi ve ürünler satıştan kaldırıldı! 🎉";

            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = _userManager.GetUserId(User);
            var offer = await _context.TradeOffers.FindAsync(id);

            if (offer == null) return NotFound();

        
            if (offer.ReceiverId != userId) return Forbid();

            offer.Status = TradeOfferStatus.Rejected;

            await _context.SaveChangesAsync();
            TempData["Info"] = "Teklifi reddettiniz.";

            return RedirectToAction(nameof(Index));
        }
    }
}