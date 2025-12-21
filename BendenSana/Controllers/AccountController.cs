using BendenSana.Models;
using BendenSana.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BendenSana.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("EditProfile", "Profile");
            }
            return View();
        }

        // 1. LOGIN SAYFASI
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
                if (result.Succeeded)
                {
                    // Giriş yaparken de role göre yönlendirme
                    if (await _userManager.IsInRoleAsync(user, "Seller"))
                    {
                        return RedirectToAction("Index", "Seller");
                    }
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.Error = "Hatalı e-posta veya şifre.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Test için Hızlı Kullanıcı Oluşturucu
        public async Task<IActionResult> CreateDemoUsers()
        {
            // 1. SATICI OLUŞTUR 
            if (await _userManager.FindByEmailAsync("satici@test.com") == null)
            {
                var seller = new ApplicationUser
                {
                    UserName = "satici@test.com",
                    Email = "satici@test.com",
                    EmailConfirmed = true,
                    FirstName = "Test",
                    LastName = "Satıcı"
                };
                var res = await _userManager.CreateAsync(seller, "Sifre123!");
                if (res.Succeeded) await _userManager.AddToRoleAsync(seller, "Seller");
            }

            // 2. ALICI OLUŞTUR 
            if (await _userManager.FindByEmailAsync("alici@test.com") == null)
            {
                var buyer = new ApplicationUser
                {
                    UserName = "alici@test.com",
                    Email = "alici@test.com",
                    EmailConfirmed = true,
                    FirstName = "Test",
                    LastName = "Alıcı"
                };
                var res = await _userManager.CreateAsync(buyer, "Sifre123!");
                if (res.Succeeded) await _userManager.AddToRoleAsync(buyer, "User");
            }

            return Content("Kullanıcılar başarıyla oluşturuldu! \n1. satici@test.com \n2. alici@test.com \nŞifreler: Sifre123!");
        }

        // PROFİL DÜZENLEME
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index");

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            return View(model);
        }

        // 2. PROFİL GÜNCELLEME İŞLEMİ (POST)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            
            model.Email = user.Email;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //  İsim ve Soyisim Güncelleme
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // Şifre Değiştirme (Eğer alanlar doluysa)
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("", "Şifrenizi değiştirmek için mevcut şifrenizi girmelisiniz.");
                    return View(model);
                }

                var changePassResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePassResult.Succeeded)
                {
                    foreach (var error in changePassResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(model);
                }
            }

            TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi.";
            
            return RedirectToAction("EditProfile");
            
        }

        // KAYIT OL (GET)
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // 4. KAYIT OL İŞLEMİ - POST (GÜNCELLENEN KISIM)
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Email mi Telefon mu kontrolü
            bool isEmail = model.EmailOrPhone.Contains("@");

            var user = new ApplicationUser
            {
                UserName = isEmail ? model.EmailOrPhone : model.EmailOrPhone,
                Email = isEmail ? model.EmailOrPhone : null,
                PhoneNumber = isEmail ? null : model.EmailOrPhone,
                FirstName = model.Name,
                LastName = "",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // === ROL ATAMA VE YÖNLENDİRME MANTIĞI ===
                string roleToAssign = "User"; // Varsayılan

                if (model.UserRole == "Seller")
                {
                    roleToAssign = "Seller";
                }
                else if (model.UserRole == "Admin")
                {
                    roleToAssign = "Admin";
                }

                // 1. Rolü Ata
                await _userManager.AddToRoleAsync(user, roleToAssign);

                // 2. Otomatik Giriş Yap
                await _signInManager.SignInAsync(user, isPersistent: false);

                // 3. Role Göre Yönlendir
                if (roleToAssign == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (roleToAssign == "Seller")
                {
                    return RedirectToAction("Index", "Seller");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}