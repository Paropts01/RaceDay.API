using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers
{
    public class EnrolmentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
