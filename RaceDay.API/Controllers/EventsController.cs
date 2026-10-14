using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers
{
    public class EventsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
