using Microsoft.AspNetCore.Mvc;

namespace F1BettingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RacesController : ControllerBase
    {
        // GET: api/races
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Races endpoint" });
        }
    }
}