using Bt.TokenManager.Model;
using Bt.TokenManager.Service;
using Microsoft.AspNetCore.Mvc;

namespace Bt.TokenManager.Controllers
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
