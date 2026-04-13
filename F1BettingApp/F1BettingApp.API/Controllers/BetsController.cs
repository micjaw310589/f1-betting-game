using Microsoft.AspNetCore.Mvc;

namespace F1BettingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BetsController : ControllerBase
    {
        // GET: api/bets
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Bets endpoint" });
        }
    }
}