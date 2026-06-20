using Microsoft.AspNetCore.Mvc;

namespace RealtorsPortal.Controllers
{
    /// <summary>
    /// Mortgage Calculator Controller
    /// Handles mortgage calculator functionality
    /// </summary>
    public class MortgageController : Controller
    {
        /// <summary>
        /// GET: /Mortgage or /Mortgage/Index
        /// Renders mortgage calculator page
        /// </summary>
        public IActionResult Index()
        {
            ViewBag.ActivePage = "Mortgage";
            return View();
        }
    }
}
