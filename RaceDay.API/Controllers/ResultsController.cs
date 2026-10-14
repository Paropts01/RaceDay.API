using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers
{
    public class ResultsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
