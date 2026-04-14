using Microsoft.AspNetCore.Mvc;

namespace BankingGateway.IdentityServer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
