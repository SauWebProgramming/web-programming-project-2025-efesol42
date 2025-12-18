using BendenSana.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    [Authorize]
    public class ConversationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConversationController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

      
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var conversations = await _context.Set<Conversation>()
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Product) 
                .Include(c => c.Messages)
                .Where(c => c.BuyerId == user.Id || c.SellerId == user.Id)
                .OrderByDescending(c => c.LastMessageDate) 
                .ToListAsync();

            return View(conversations);
        }

        
        public async Task<IActionResult> Start(string userId, int productId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            
            if (userId == currentUser.Id) return RedirectToAction("Details", "Product", new { id = productId });

            var existingConv = await _context.Set<Conversation>()
                .FirstOrDefaultAsync(c => c.ProductId == productId &&
                                          ((c.BuyerId == currentUser.Id && c.SellerId == userId) ||
                                           (c.BuyerId == userId && c.SellerId == currentUser.Id)));

            if (existingConv != null)
            {
                return RedirectToAction("Details", new { id = existingConv.Id });
            }

            
            var newConv = new Conversation
            {
                BuyerId = currentUser.Id,
                SellerId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow,
                LastMessageDate = DateTime.UtcNow
            };

            _context.Set<Conversation>().Add(newConv);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = newConv.Id });
        }

   
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var conversation = await _context.Set<Conversation>()
                .Include(c => c.Messages)
                .Include(c => c.Product) 
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conversation == null) return NotFound();

          
            if (conversation.BuyerId != user.Id && conversation.SellerId != user.Id)
            {
                return Forbid();
            }

            return View(conversation);
        }

       
        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return RedirectToAction("Details", new { id = conversationId });

            var user = await _userManager.GetUserAsync(User);

            
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = user.Id,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Set<Message>().Add(message);

            var conversation = await _context.Set<Conversation>().FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.LastMessageDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = conversationId });
        }
    }
}