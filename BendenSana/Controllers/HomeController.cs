using BendenSana.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BendenSana.Controllers
{
    public class HomeController : Controller
    {

        public HomeController()
        {
        }

        public IActionResult Index()
        {

            return View();
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