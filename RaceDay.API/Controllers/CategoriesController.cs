using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
