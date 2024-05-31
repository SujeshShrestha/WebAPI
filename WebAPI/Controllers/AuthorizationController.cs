using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using WebAPI.Models;
using WebAPI.Models.Domain;
using WebAPI.Models.DTO;
using WebAPI.Repositories.Abstract;

namespace WebAPI.Controllers
{
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly DatabaseContext _context;
        public AuthorizationController(DatabaseContext context,
            RoleManager<ApplicationUser> roleManager,
            UserManager<ApplicationUser> usermanager,
            ITokenService tokenService)
        {
            _context = context;
            _userManager = usermanager;
            roleManager = roleManager;
            _tokenService = tokenService;

        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles= await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())


                };
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));

                }
                var token = _tokenService.GetToken(authClaims);
                var refreshToken = _tokenService.GetRefreshToken();
                var tokenInfo=_context.TokenInfo.FirstOrDefault(x=>x.UserName == user.UserName);
                if (tokenInfo == null)
                {
                    var info = new TokenInfo
                    {
                        UserName = user.UserName,
                        RefreshToken = refreshToken,
                        RefreshTokenExpiry = DateTime.Now.AddDays(7)
                    };
                    
                }
            else
            {
                tokenInfo.RefreshToken = refreshToken;
                tokenInfo.RefreshTokenExpiry= DateTime.Now.AddDays(7);
            }
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {

                    return BadRequest(ex.Message);
                }

            return Ok(new LoginResponse
            {
                Name=user.Name,
                UserName=user.UserName,
                Token=token.TokenString,
                RefreshToken=refreshToken,
                Expiration=token.ValidTo,
                StatusCode=1,
                Message="Logged in"
            });
            }
            return Ok(new LoginResponse
            {
                StatusCode = 0,
                Message = "Invalid Username or Password",
                Token = "", Expiration = null
            });
        }
       
    //    public async Task<IActionResult> Registration([FromBody] RegistrationModel register)
    //    {
    //        var status = new Status();
    //        if (!ModelState.IsValid)
    //        {
    //            status.StatusCode = 0;
    //            status.Message = "Please pass all the required field";
    //            return Ok(status);
    //        }
    //        //check if user exixts
    //        var userExists = _userManager.FindByNameAsync(register.Username);
    //        if (userExists == null)
    //        {
    //            status.StatusCode = 0;
    //            status.Message = "Invalid userName";
    //            return Ok(status);

    //        }
    //        var user = new ApplicationUser
    //        {
    //            UserName = register.Username,
    //            SecurityStamp = Guid.NewGuid().ToString(),
    //            Email = register.Email,
    //            Name = register.Name
    //        };
    //        var result = await _userManager.CreateAsync(user, register.Password);
    //        if (!result.Succeeded)
    //        {
    //            status.StatusCode = 0;
    //            status.Message = "User Creation Failed";
    //            return Ok(status);


    //        }
    //        /// adding roles
    //        /// for admin user use- UserRoles.Admin 
    //        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
    //            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
    //        if (await _roleManager.RoleExistsAsync(UserRoles.User))
    //        {
    //            await _userManager.AddToRoleAsync(user, UserRoles.User);
    //        }
    //        status.StatusCode = 1;
    //        status.Message = "User SuccessFully Registered";
    //        return Ok(status);
    //    }


    }
}
