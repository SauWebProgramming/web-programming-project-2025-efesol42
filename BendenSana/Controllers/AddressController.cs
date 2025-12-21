using BendenSana.Models.Repositories;
using BendenSana.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace BendenSana.Controllers
{
    [Authorize]
    public class AddressController : Controller
    {
        private readonly IAddressRepository _addressRepo; // Yeni bağımlılık
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressController(IAddressRepository addressRepo, UserManager<ApplicationUser> userManager)
        {
            _addressRepo = addressRepo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var address = await _addressRepo.GetByUserIdAsync(user.Id); // Repo kullanımı

            var model = new AddressViewModel
            {
                ZipCode = address?.ZipCode,
                Country = address?.Country,
                City = address?.City,
                AddressDetail = address?.AddressLine,
                District = address?.AddressLine2
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(AddressViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var address = await _addressRepo.GetByUserIdAsync(user.Id);

            if (address == null)
            {
                address = new Address
                {
                    UserId = user.Id,
                    Title = "Varsayılan Adres",
                    ZipCode = model.ZipCode,
                    Country = model.Country,
                    City = model.City,
                    AddressLine = model.AddressDetail,
                    AddressLine2 = model.District
                };
                await _addressRepo.AddAsync(address);
            }
            else
            {
                address.ZipCode = model.ZipCode;
                address.Country = model.Country;
                address.City = model.City;
                address.AddressLine = model.AddressDetail;
                address.AddressLine2 = model.District;
                await _addressRepo.UpdateAsync(address);
            }

            await _addressRepo.SaveChangesAsync();
            TempData["Success"] = "Adres defteriniz başarıyla güncellendi.";
            return RedirectToAction("Index");
        }
    }
}

