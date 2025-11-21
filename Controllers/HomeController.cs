using Microsoft.AspNetCore.Mvc;


namespace Part2_CMCS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}