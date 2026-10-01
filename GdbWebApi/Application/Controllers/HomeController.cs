using GDB.App.Application.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GdbWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var message = _homeService.GetWelcomeMessage();
            return Ok(new { message });
        }
    }
}
