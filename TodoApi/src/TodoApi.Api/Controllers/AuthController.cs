using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Api.Models;
using TodoApi.Application.Interfaces;

namespace TodoApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public AuthController(IAuthService authService, IJwtTokenGenerator tokenGenerator)
        {
            _authService = authService;
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (isValid, userId, claims) = await _authService.ValidateCredentialsAsync(request.Username, request.Password);

            if (!isValid)
            {
                return Unauthorized();
            }

            // Ensure sub claim is present
            var claimList = new List<Claim>(claims ?? Enumerable.Empty<Claim>());
            if (!claimList.Any(c => c.Type == ClaimTypes.NameIdentifier || c.Type == ClaimTypes.Name))
            {
                claimList.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

            var token = _tokenGenerator.GenerateToken(userId, claimList);

            return Ok(new { token });
        }
    }
}
