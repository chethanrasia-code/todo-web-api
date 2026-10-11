using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TodoApi.Application.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Validate username/password and return whether valid, the user id and any claims to include in the token.
        /// </summary>
        Task<(bool IsValid, string UserId, IEnumerable<Claim> Claims)> ValidateCredentialsAsync(string username, string password);
    }
}
