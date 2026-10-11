using System.Security.Claims;

namespace TodoApi.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(string userId, IEnumerable<Claim> claims);
    }
}
