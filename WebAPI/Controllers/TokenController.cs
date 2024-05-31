using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Domain;
using WebAPI.Models.DTO;
using WebAPI.Repositories.Abstract;

namespace WebAPI.Controllers
{
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly DatabaseContext _databaseContext;
        public TokenController(DatabaseContext databaseContext,
            ITokenService tokenService)
        {
            _databaseContext = databaseContext;
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task< IActionResult> Refresh(RequestTokenRefresh refreshToken)
        {
            // Check if the provided refresh token is null
            if (refreshToken == null)
            {
                return BadRequest("Invalid client request");
            }

            string accessToken = refreshToken.AccessToken;
            string refreshTokeen = refreshToken.RefreshToken;

            // Get the principal claims from the access token
            var principal = _tokenService.GetClaimPrincipal(accessToken);
            if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
            {
                return BadRequest("Invalid client request");
            }

            string userName = principal.Identity.Name;

            // Retrieve the user token info from the database
            var user = _databaseContext.TokenInfo.SingleOrDefault(x => x.UserName == userName);
            if (user == null || user.RefreshToken != refreshTokeen || user.RefreshTokenExpiry <= DateTime.Now)
            {
                return BadRequest("Invalid client request");
            }

            // Generate new tokens
            var newAccessToken = _tokenService.GetToken(principal.Claims);
            var newRefreshToken = _tokenService.GetRefreshToken();

            // Update the user's refresh token and save changes
            user.RefreshToken = newRefreshToken;
            _databaseContext.SaveChanges();

            // Return the new tokens
            return Ok(new RequestTokenRefresh
            {
                AccessToken = newAccessToken.TokenString,
                RefreshToken = newRefreshToken
            });
        }

        //revoking user  use the token entry
       
        //public IActionResult Revoke()
        //{
        //    try
        //    {
        //        var userName = User.Identity.Name;
        //        var user = _databaseContext.TokenInfo.SingleOrDefault(x => x.UserName == userName);
        //        if (user != null)
        //        {
        //            user.RefreshToken = null;
        //            _databaseContext.SaveChanges();
        //            return Ok(true);
        //        }

        //    }
        //    catch (Exception)
        //    {

        //        return BadRequest();
        //    }
            
        //    return BadRequest();

        //}

    }
}
