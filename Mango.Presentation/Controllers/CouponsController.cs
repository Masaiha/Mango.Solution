using Mango.Presentation.Interfaces;
using Mango.Presentation.Models.Dtos;
using Mango.Presentation.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

            var teste = JsonUtils.Deserialize<List<CouponsDto>>(coupons.Result?.ToString());


            return Ok(coupons);
        }
    }
}
