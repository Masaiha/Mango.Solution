using Mango.Presentation.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Presentation.Controllers
{
    public class CouponsController : Controller
    {
        private readonly ICouponService _service;

        public CouponsController(ICouponService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var teste = await _service.GetAll();

            return View();
        }

        public async Task<IActionResult> GetAll()
        {
            var coupons = await _service.GetAll();

            return Ok(coupons);
        }
    }
}
