using BendenSana.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BendenSana.Controllers
{

    [Authorize]
    public class ConversationController : Controller
    {
        private readonly IConversationRepository _conversationRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConversationController(IConversationRepository conversationRepo, UserManager<ApplicationUser> userManager)
        {
            _conversationRepo = conversationRepo;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var conversations = await _conversationRepo.GetUserConversationsAsync(user.Id);
            return View(conversations);
        }

        public async Task<IActionResult> Start(string userId, int productId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            if (userId == currentUser.Id)
                return RedirectToAction("Details", "Product", new { id = productId });

            var existingConv = await _conversationRepo.FindExistingConversationAsync(currentUser.Id, userId, productId);

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

            await _conversationRepo.AddConversationAsync(newConv);
            await _conversationRepo.SaveChangesAsync();

            return RedirectToAction("Details", new { id = newConv.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var conversation = await _conversationRepo.GetConversationWithDetailsAsync(id);

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
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", new { id = conversationId });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = user.Id,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            await _conversationRepo.AddMessageAsync(message);

            var conversation = await _conversationRepo.GetByIdAsync(conversationId);
            if (conversation != null)
            {
                conversation.LastMessageDate = DateTime.UtcNow;
            }

            await _conversationRepo.SaveChangesAsync();

            return RedirectToAction("Details", new { id = conversationId });
        }
    }
}
