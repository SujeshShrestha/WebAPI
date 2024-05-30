using System.Security.Claims;
using WebAPI.Models.DTO;

namespace WebAPI.Repositories.Abstract
{
    public interface ITokenService
    {
        TokenResponse GetToken(IEnumerable<Claim> claim);
        string GetRefreshToken();
        ClaimsPrincipal GetClaimPrincipal(string token);
    }
}
