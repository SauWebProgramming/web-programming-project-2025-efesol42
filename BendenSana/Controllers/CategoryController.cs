using BendenSana.Models;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Set<global::Category>().ToListAsync();
            return View(categories);
        }

        // 2. YENİ KATEGORİ EKLE 
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. YENİ KATEGORİ KAYDET (POST)
        [HttpPost]
        public async Task<IActionResult> Create(global::Category category, IFormFile? imageFile)
        {
            if (imageFile != null)
            {
                
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "category_images");

                
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }


                string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, fileName);

               
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

             
                category.ImageUrl = "/category_images/" + fileName;
            }

            _context.Set<global::Category>().Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DÜZENLEME SAYFASI (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Set<global::Category>().FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // GÜNCELLEME İŞLEMİ (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, global::Category category, IFormFile? imageFile)
        {
            if (id != category.Id) return NotFound();

            if (imageFile != null)
            {
                
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "category_images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                category.ImageUrl = "/category_images/" + fileName;
            }
          

            _context.Update(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // SİLME İŞLEMİ
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Set<global::Category>().FindAsync(id);
            if (category != null)
            {
                _context.Set<global::Category>().Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}