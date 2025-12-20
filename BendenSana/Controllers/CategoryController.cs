using BendenSana.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// System.IO kütüphanesini eklediğinden emin ol

namespace BendenSana.Controllers
{
    // [Authorize(Roles = "Admin")] 
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. KATEGORİ LİSTESİ 
        // Mevcut Index metodunu bununla değiştir veya yoksa ekle:
        public async Task<IActionResult> Index()
        {
            // Sadece Ana Kategorileri (ParentId'si olmayanları) listeliyoruz
            var categories = await _context.Categories
                .Where(c => c.ParentId == null)
                .Include(c => c.Children) // Alt kategorisi varsa sayısını göstermek için
                .AsNoTracking()
                .ToListAsync();

            return View(categories);
        }

        // 2. YENİ KATEGORİ EKLE 
        [HttpGet]
        public IActionResult Create()
        {
            // Tüm kategorileri çekip ViewBag'e atıyoruz ki dropdown'da gösterebilelim
            ViewBag.Parents = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Categories.Where(c => c.ParentId == null).ToList(), "Id", "Name");
            return View();
        }

        // 3. YENİ KATEGORİ KAYDET (POST)
        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik önlemi (CSRF saldırılarına karşı)
        public async Task<IActionResult> Create(Category category, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    // Resim yükleme işlemini metoda taşıdık
                    category.ImageUrl = await UploadImageAsync(imageFile);
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // 4. DÜZENLEME SAYFASI (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            // Tüm kategorileri çekip ViewBag'e atıyoruz ki dropdown'da gösterebilelim
            ViewBag.Parents = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Categories.Where(c => c.ParentId == null).ToList(), "Id", "Name");
            return View(category);
        }

        // 5. GÜNCELLEME İŞLEMİ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile? imageFile)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Güncellenecek veriyi önce veritabanından çekiyoruz (Takip edilen entity)
                    var existingCategory = await _context.Categories.FindAsync(id);

                    if (existingCategory == null) return NotFound();

                    // Manuel property eşleştirmesi (Veri kaybını önlemek için)
                    existingCategory.Name = category.Name;
                    existingCategory.ParentId = category.ParentId;
                   
                    // Diğer alanları buraya ekle...

                    if (imageFile != null)
                    {
                        // 1. Eski resmi sunucudan sil
                        DeleteImage(existingCategory.ImageUrl);

                        // 2. Yeni resmi yükle
                        existingCategory.ImageUrl = await UploadImageAsync(imageFile);
                    }
                    // Not: Resim yüklenmediyse existingCategory.ImageUrl değişmez, eski hali korunur.

                    _context.Update(existingCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == category.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // 6. SİLME İŞLEMİ
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // Kategoriyi silmeden önce resmini de klasörden siliyoruz
                DeleteImage(category.ImageUrl);

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- YARDIMCI METOTLAR (Kod tekrarını önlemek için) ---

        // Resim Yükleme Metodu
        private async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "category_images");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/category_images/" + uniqueFileName;
        }

        // Resim Silme Metodu
        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            // URL veritabanında "/category_images/abc.jpg" şeklinde kayıtlı.
            // Bunu fiziksel yola (C:\Sites\wwwroot\category_images\abc.jpg) çevirmemiz lazım.

            // Başındaki slash'ı kaldırıp wwwroot yoluna ekliyoruz
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
    }
}