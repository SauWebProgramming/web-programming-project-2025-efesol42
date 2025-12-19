using BendenSana.Models;
using BendenSana.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BendenSana.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public IActionResult Index()
        {
            var recentProducts = _productRepository.GetProductsWithCategories()
                .Where(p => p.Status == ProductStatus.available)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToList();

            return View(recentProducts);
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string name, string email, string phone, string message)
        {
            // Burada mesajý veritabanýna kaydedebilir veya e-posta gönderebilirsiniz.
            // Þimdilik sadece baþarýlý mesajý gösterelim.
            TempData["Success"] = "Mesajýnýz baþarýyla gönderildi! En kýsa sürede dönüþ yapacaðýz.";
            return RedirectToAction("Contact");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}