using BendenSana.Models;
using Microsoft.AspNetCore.Authorization;
using BendenSana.Models.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BendenSana.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepo;

        public HomeController(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<IActionResult> Index()
        {
            // Ana sayfa için son 6 ürünü getiriyoruz
            var products = await _productRepo.GetHomeProductsAsync(6);
            return View(products);
        }

        public IActionResult About()
        {
            return View();
        }

        [Authorize(Roles = "Seller, User")]
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [Authorize(Roles ="Seller, User")]        
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