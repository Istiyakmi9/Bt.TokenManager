using Bot.App.ServiceManager.Model;
using Bot.App.ServiceManager.Service;
using Microsoft.AspNetCore.Mvc;

namespace Bot.App.ServiceManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenManagerController : ControllerBase
    {
        private readonly TokenManagerService _tokenManagerService;

        public TokenManagerController(TokenManagerService tokenManagerService)
        {
            _tokenManagerService = tokenManagerService;
        }

        [HttpPost("generateToken")]
        public async Task<string> GenerateToken(RequestToken requestToken)
        {
            return await _tokenManagerService.GenerateAccessToken(requestToken);
        }
    }
}
