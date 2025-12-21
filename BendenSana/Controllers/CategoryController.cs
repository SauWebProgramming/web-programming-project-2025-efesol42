using BendenSana.Models.Repositories;
using BendenSana.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{

    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IFileService _fileService;

        public CategoryController(ICategoryRepository categoryRepo, IFileService fileService)
        {
            _categoryRepo = categoryRepo;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepo.GetParentCategoriesAsync();
            return View(categories);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var parents = await _categoryRepo.GetParentCategoriesAsync();
            ViewBag.Parents = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(parents, "Id", "Name");
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                    category.ImageUrl = await _fileService.UploadImageAsync(imageFile, "category_images");

                await _categoryRepo.AddAsync(category);
                return RedirectToAction("Category", "Admin");
            }
            return RedirectToAction("Category", "Admin");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();

            var parents = await _categoryRepo.GetParentCategoriesAsync();
            ViewBag.Parents = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(parents, "Id", "Name");
            return View(category);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile? imageFile)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _categoryRepo.GetByIdAsync(id);
                    if (existingCategory == null) return NotFound();

                    existingCategory.Name = category.Name;
                    existingCategory.ParentId = category.ParentId;

                    if (imageFile != null)
                    {
                        _fileService.DeleteImage(existingCategory.ImageUrl);
                        existingCategory.ImageUrl = await _fileService.UploadImageAsync(imageFile, "category_images");
                    }

                    await _categoryRepo.UpdateAsync(existingCategory);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _categoryRepo.ExistsAsync(category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Category", "Admin");

            }
            return View(category);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category != null)
            {
                _fileService.DeleteImage(category.ImageUrl);
                await _categoryRepo.DeleteAsync(id);
            }
            return RedirectToAction("Category", "Admin");
        }
    }
}
