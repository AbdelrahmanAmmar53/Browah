using Berwah.BL.Service.IService;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpGet("GetHomeData")]
        public async Task<IActionResult> GetHomeData()
        {
           
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _homeService.GetHomeDataAsync(userId);
            return Ok(result);
        }
    }
}
