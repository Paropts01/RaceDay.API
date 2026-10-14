using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
