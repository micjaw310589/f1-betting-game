using Microsoft.AspNetCore.Mvc;

namespace F1BettingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login()
        {
            return Ok(new { message = "Auth login endpoint" });
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public IActionResult Register()
        {
            return Ok(new { message = "Auth register endpoint" });
        }
    }
}