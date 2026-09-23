using Berwah.BL.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;
        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }
        [HttpGet("GetHomeData")]
        public async Task<IActionResult> GetHomeData()
        {
            var homeData = await _homeService.GetHomeDataAsync();
            return Ok(homeData);
        }
    }
}
