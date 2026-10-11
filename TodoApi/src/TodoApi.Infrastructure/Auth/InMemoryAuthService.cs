using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoApi.Application.Interfaces;

namespace TodoApi.Infrastructure.Auth
{
    /// <summary>
    /// Simple in-memory authentication for development and tests only.
    /// Replace with a proper user store and password hashing for production.
    /// </summary>
    public class InMemoryAuthService : IAuthService
    {
        // very small in-memory user store: username -> (userId, password)
        private readonly Dictionary<string, (string UserId, string Password)> _users =
            new()
            {
                { "admin", ("1", "P@ssw0rd") },
                { "user", ("2", "password") }
            };

        public Task<(bool IsValid, string UserId, IEnumerable<Claim> Claims)> ValidateCredentialsAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Task.FromResult((false, (string?)null, (IEnumerable<Claim>)new Claim[0]));
            }

            if (_users.TryGetValue(username, out var entry) && entry.Password == password)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, entry.UserId)
                };

                return Task.FromResult((true, entry.UserId, (IEnumerable<Claim>)claims));
            }

            return Task.FromResult((false, (string?)null, (IEnumerable<Claim>)new Claim[0]));
        }
    }
}
