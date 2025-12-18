using BendenSana.Models;
using BendenSana.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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


        public async Task<IActionResult> Index(string search, int? categoryId)
        {
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => p.Title.ToLower().Contains(search.ToLower()));
            }

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await productsQuery.ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .Include(p => p.Reviews)        
                    .ThenInclude(r => r.User)   
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                ViewBag.IsFavorite = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == id);
            }
            else
            {
                ViewBag.IsFavorite = false;
            }

            return View(product);
        }

       
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
            return RedirectToAction("Index");
        }

   
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (product.SellerId != user.Id && !User.IsInRole("Admin")) return Forbid();

            var model = new ProductCreateViewModel
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId
            };

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(ProductCreateViewModel model)
        {
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

                        var productImage = new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = "/images/products/" + uniqueFileName
                        };
                        _context.ProductImages.Add(productImage);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Ürün güncellendi.";
            return RedirectToAction("Details", new { id = product.Id });
        }

        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

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