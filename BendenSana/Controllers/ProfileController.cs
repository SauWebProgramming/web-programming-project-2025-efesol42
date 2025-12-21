using BendenSana.Models;
using BendenSana.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BendenSana.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Profile/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Account");

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Address = user.Address
            };

            return View(model);
        }

        // POST: /Profile/EditProfile
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Account");

            // Email alanını kullanıcıdan gelen veriyle değil, veritabanındaki gerçek veriyle doldur (güvenlik)
            model.Email = user.Email;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Temel Bilgileri Güncelle
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Address = model.Address; // Adres güncellemesi

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // 2. Şifre Değişikliği (Eğer alanlar doluysa)
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Şifre değiştirmek için mevcut şifrenizi girmelisiniz.");
                    return View(model);
                }

                var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
                else
                {
                    // Şifre değişince oturumun düşmemesi için cookie'yi yenile
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["Success"] = "Profil ve şifreniz başarıyla güncellendi.";
                }
            }
            else
            {
                TempData["Success"] = "Profil bilgileriniz güncellendi.";
            }

            return RedirectToAction("EditProfile");
        }
    }
}