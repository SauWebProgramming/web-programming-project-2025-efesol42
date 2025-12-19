using BendenSana.Models;
using BendenSana.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- 1. GELİŞMİŞ ARAMA VE FİLTRELEME (Search Sayfası) ---
        // Home sayfasındaki arama çubuğu buraya istek atacak: asp-controller="Product" asp-action="Search"
        [HttpGet]
        public async Task<IActionResult> Search(SearchViewModel model)
        {
            // 1. Ana Sorgu
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Color)
                .Where(p => p.Status == ProductStatus.available)
                .AsQueryable();

            // 2. Arama Kelimesi
            if (!string.IsNullOrEmpty(model.SearchQuery))
            {
                var lowerQuery = model.SearchQuery.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(lowerQuery) ||
                                         p.Description.ToLower().Contains(lowerQuery));
            }

            // 3. Kategori Filtresi
            if (model.SelectedCategoryIds != null && model.SelectedCategoryIds.Any())
            {
                query = query.Where(p => model.SelectedCategoryIds.Contains(p.CategoryId));
            }

            // 4. Renk Filtresi
            if (model.SelectedColorIds != null && model.SelectedColorIds.Any())
            {
                query = query.Where(p => p.ColorId.HasValue && model.SelectedColorIds.Contains(p.ColorId.Value));
            }

            // 5. Cinsiyet Filtresi
            if (model.SelectedGenders != null && model.SelectedGenders.Any())
            {
                query = query.Where(p => p.Gender.HasValue && model.SelectedGenders.Contains(p.Gender.Value));
            }

            // 6. Fiyat
            if (model.MinPrice.HasValue) query = query.Where(p => p.Price >= model.MinPrice.Value);
            if (model.MaxPrice.HasValue) query = query.Where(p => p.Price <= model.MaxPrice.Value);

            // 7. Sıralama
            query = model.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // 8. Sonuçları Doldur
            model.Products = await query.ToListAsync();
            model.TotalResults = model.Products.Count;

            // 9. View'ın hata vermemesi için Dropdownları Doldur (Çok Önemli)
            model.AllCategories = await _context.Categories
                .Where(c => c.ParentId == null)
                .Include(c => c.Children)
                .ToListAsync();

            model.AllColors = await _context.Set<Color>().ToListAsync();

            return View(model); // Views/Product/Search.cshtml sayfasına gider
        }

        // --- 2. VARSAYILAN INDEX (İstersen Search'e yönlendir) ---
        public IActionResult Index()
        {
            return RedirectToAction("Search");
        }

        // --- 3. SATICININ ÜRÜNLERİM SAYFASI ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyProducts()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var myProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.SellerId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(myProducts);
        }

        // --- 4. ÜRÜN DETAY ---
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.Color)
                .Include(p => p.Seller)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // Benzer Ürünler
            var relatedProducts = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id && p.Status == ProductStatus.available)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            // Favori Kontrolü
            var userId = _userManager.GetUserId(User);
            ViewBag.IsFavorite = false;
            if (userId != null)
            {
                ViewBag.IsFavorite = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == id);
            }

            return View(product);
        }

        // --- 5. YORUM EKLEME ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview(int productId, int? rating, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _context.Products.FindAsync(productId);

            if (product == null) return NotFound();
            if (product.SellerId == user.Id)
            {
                TempData["Error"] = "Kendi ürününüze yorum yapamazsınız.";
                return RedirectToAction("Details", new { id = productId });
            }

            if (rating == null && string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Lütfen puan verin veya yorum yazın.";
                return RedirectToAction("Details", new { id = productId });
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = user.Id,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Değerlendirmeniz eklendi.";
            return RedirectToAction("Details", new { id = productId });
        }

        // --- 6. RAPORLAMA ---
        [Authorize]
        [HttpGet]
        public IActionResult Report(int id)
        {
            ViewBag.ProductId = id;
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Report(int productId, string reason, string description)
        {
            var user = await _userManager.GetUserAsync(User);
            var report = new ProductReport
            {
                ProductId = productId,
                ReporterId = user.Id,
                Reason = reason,
                Description = description,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductReports.Add(report);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Şikayetiniz alındı.";
            return RedirectToAction("Details", new { id = productId });
        }

        // --- 7. YENİ ÜRÜN EKLEME ---
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var product = new Product
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                CategoryId = model.CategoryId,
                SellerId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Status = ProductStatus.available
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (model.Photos != null && model.Photos.Count > 0)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                foreach (var file in model.Photos)
                {
                    if (file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        var productImage = new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = "/images/products/" + uniqueFileName
                        };
                        _context.ProductImages.Add(productImage);
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "İlanınız yayınlandı.";
            return RedirectToAction("Index"); // Search sayfasına yönlendirir
        }

        // --- 8. DÜZENLEME (EDIT) ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (product.SellerId != user.Id && !User.IsInRole("Admin")) return Forbid();

            var model = new ProductCreateViewModel
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ExistingImageUrls = product.Images.Select(i => i.ImageUrl).ToList()
            };

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            var product = await _context.Products.FindAsync(model.Id);
            if (product == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (product.SellerId != user.Id && !User.IsInRole("Admin")) return Forbid();

            product.Title = model.Title;
            product.Description = model.Description;
            product.Price = model.Price;
            product.CategoryId = model.CategoryId;

            if (model.Photos != null && model.Photos.Count > 0)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                foreach (var file in model.Photos)
                {
                    if (file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        var productImage = new ProductImage { ProductId = product.Id, ImageUrl = "/images/products/" + uniqueFileName };
                        _context.ProductImages.Add(productImage);
                    }
                }
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Ürün güncellendi.";
            return RedirectToAction("Details", new { id = product.Id });
        }

        // --- 9. SİLME (DELETE) ---
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (product.SellerId != user.Id && !User.IsInRole("Admin")) return Forbid();

            if (product.Images != null)
            {
                foreach (var img in product.Images)
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ürün silindi.";
            return RedirectToAction("Index");
        }
    }
}