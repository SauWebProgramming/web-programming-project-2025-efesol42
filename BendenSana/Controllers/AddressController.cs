using BendenSana.Models;
using BendenSana.ViewModels; // ViewModel'i kullanmak için gerekli
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

        // GET: /Address/Index (Adres Düzenleme Sayfası)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Kullanıcının kayıtlı ilk adresini getir (Tasarımda tek adres var gibi görünüyor)
            var address = await _context.Set<Address>().FirstOrDefaultAsync(a => a.UserId == user.Id);

            // Mevcut verileri ViewModel'e doldur
            var model = new AddressViewModel
            {
                ZipCode = address?.ZipCode,
                Country = address?.Country,
                City = address?.City,
                AddressDetail = address?.AddressLine, // Detaylı adres
                District = address?.AddressLine2 // Mahalle/İlçe olarak AddressLine2'yi kullanıyoruz
            };

            return View(model);
        }

        // POST: /Address/Index (Kaydetme İşlemi)
        [HttpPost]
        public async Task<IActionResult> Index(AddressViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Veritabanındaki adresi kontrol et
            var address = await _context.Set<Address>().FirstOrDefaultAsync(a => a.UserId == user.Id);

            if (address == null)
            {
                // Adres yoksa YENİ OLUŞTUR
                address = new Address
                {
                    UserId = user.Id,
                    Title = "Varsayılan Adres", // Otomatik başlık
                    ZipCode = model.ZipCode,
                    Country = model.Country,
                    City = model.City,
                    AddressLine = model.AddressDetail,
                    AddressLine2 = model.District
                };
                _context.Set<Address>().Add(address);
            }
            else
            {
                // Adres varsa GÜNCELLE
                address.ZipCode = model.ZipCode;
                address.Country = model.Country;
                address.City = model.City;
                address.AddressLine = model.AddressDetail;
                address.AddressLine2 = model.District;

                _context.Set<Address>().Update(address);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Adres defteriniz başarıyla güncellendi.";

            // İşlem bitince aynı sayfada kal (bilgileri göster)
            return RedirectToAction("Index");
        }

        // Opsiyonel: Silme işlemi (Tasarımda buton yok ama backend'de kalabilir)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var address = await _context.Set<Address>().FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (address != null)
            {
                _context.Set<Address>().Remove(address);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Adres silindi.";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Yeni oluştururken boş bir model gönderiyoruz
            return View(new AddressViewModel());
        }

        // POST: /Address/Create (Kaydetme İşlemi)
        [HttpPost]
        public async Task<IActionResult> Create(AddressViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Yeni Adres Nesnesi Oluştur
            var address = new Address
            {
                UserId = user.Id,
                Title = "Yeni Adres", // İstersen ViewModel'e Title alanı ekleyip oradan alabilirsin
                ZipCode = model.ZipCode,
                Country = model.Country,
                City = model.City,
                AddressLine = model.AddressDetail, // ViewModel -> Entity eşleşmesi
                AddressLine2 = model.District      // ViewModel -> Entity eşleşmesi
            };

            _context.Set<Address>().Add(address);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yeni adres başarıyla eklendi.";

            // İşlem bitince listeye (Index) dön
            return RedirectToAction("Index");
        }
    }
}