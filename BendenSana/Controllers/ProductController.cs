using BendenSana.Models;
using BendenSana.Models.Repositories;
using BendenSana.Repositories;
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
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context; // Renkler vb. için geçici

        public ProductController(IProductRepository productRepo, ICategoryRepository categoryRepo,
                                 IFileService fileService, UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _fileService = fileService;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Search(SearchViewModel model)
        {
            model.Products = await _productRepo.SearchProductsAsync(model);
            model.TotalResults = model.Products.Count;
            model.AllCategories = await _categoryRepo.GetParentCategoriesAsync();
            model.AllColors = await _context.Set<Color>().ToListAsync();
            return View(model);
        }

        public IActionResult Index() => RedirectToAction("Search");

        [Authorize]
        public async Task<IActionResult> MyProducts()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(await _productRepo.GetMyProductsAsync(user.Id));
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepo.GetDetailsAsync(id);
            if (product == null) return NotFound();

            ViewBag.RelatedProducts = await _productRepo.GetRelatedProductsAsync(product.CategoryId, id, 4);
            var userId = _userManager.GetUserId(User);
            ViewBag.IsFavorite = userId != null && await _productRepo.IsFavoriteAsync(userId, id);

            return View(product);
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> AddReview(int productId, int? rating, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null) return NotFound();

            if (product.SellerId == user.Id)
            {
                TempData["Error"] = "Kendi ürününüze yorum yapamazsınız.";
                return RedirectToAction("Details", new { id = productId });
            }

            await _productRepo.AddReviewAsync(new Review
            {
                ProductId = productId,
                UserId = user.Id,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            });
            await _productRepo.SaveChangesAsync();
            return RedirectToAction("Details", new { id = productId });
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _categoryRepo.GetAllAsync(), "Id", "Name");
            return View();
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _categoryRepo.GetAllAsync(), "Id", "Name");
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

            await _productRepo.AddAsync(product);
            await _productRepo.SaveChangesAsync();

            if (model.Photos?.Any() == true)
            {
                foreach (var file in model.Photos)
                {
                    var url = await _fileService.UploadImageAsync(file, "images/products");
                    await _productRepo.AddImageAsync(new ProductImage { ProductId = product.Id, ImageUrl = url });
                }
                await _productRepo.SaveChangesAsync();
            }

            return RedirectToAction("Search");
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepo.GetDetailsAsync(id);
            if (product == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (product.SellerId != user.Id && !User.IsInRole("Admin")) return Forbid();

            foreach (var img in product.Images) _fileService.DeleteImage(img.ImageUrl);

            await _productRepo.DeleteAsync(product);
            await _productRepo.SaveChangesAsync();
            return RedirectToAction("Search");
        }
    }
}