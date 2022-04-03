using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using System.Threading.Tasks;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([FromRoute(Name = "id")] int userId)
        {
            var users = await _userService.GetUserByIdAsync(userId);
            return Ok(users);
        }
    }
}
