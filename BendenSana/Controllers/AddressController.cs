using BendenSana.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    [Authorize]
    public class AddressController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Create(Address address)
        {
            var user = await _userManager.GetUserAsync(User);
            address.UserId = user.Id;

           
            _context.Set<Address>().Add(address);
            await _context.SaveChangesAsync();

            if (Request.Query.ContainsKey("returnUrl"))
            {
                return Redirect(Request.Query["returnUrl"]);
            }

            return RedirectToAction("Index");
        }

       
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var addresses = await _context.Set<Address>().Where(a => a.UserId == user.Id).ToListAsync();
            return View(addresses);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var address = await _context.Set<Address>().FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
            if (address != null)
            {
                _context.Set<Address>().Remove(address);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}