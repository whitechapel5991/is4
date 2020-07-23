using IdentityModel;
using IdentityServer.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticateController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IWebHostEnvironment _environment;
        //private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly UserManager<ApplicationUser> _userManager;

        public AuthenticateController(
            IIdentityServerInteractionService interaction,
            //UserManager<ApplicationUser> userManager,
            // SignInManager<ApplicationUser> signInManager, 
             IWebHostEnvironment environment)
        {
            //_userManager = userManager;
            _interaction = interaction;
            _environment = environment;
            //_signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody]LoginRequest request)
        {
            var context = await _interaction.GetAuthorizationContextAsync(request.ReturnUrl);
            //var user = await _userManager.FindByEmailAsync(request.Email);
            var user = Config.GetTestUsers()
                   .FirstOrDefault(usr => usr.Password == request.Password && usr.Claims.Any(x => x.Type == JwtClaimTypes.Email && x.Value == request.Email));

            if (user != null)
            {
                //var result = await _signInManager.PasswordSignInAsync(user, request.Password, isPersistent: true, lockoutOnFailure: true);

                //var user2 = Config.GetTestUsers()
                //       .FirstOrDefault(usr => usr.Password == request.Password && usr.Claims.Any(x => x.Type == JwtClaimTypes.Email && x.Value == request.Email));
                

                if (/*result.Succeeded &&*/ context != null)
                {
                    //await HttpContext.SignInAsync(user.SubjectId, user.Username);
                    return new JsonResult(new
                    {
                        RedirectUrl = request.ReturnUrl, IsOk = true });
                    }
            }
            

            return Unauthorized();
        }


        [HttpGet]
		[Route("Logout")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var context = await _interaction.GetLogoutContextAsync(logoutId);
            bool showSignoutPrompt = true;

            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                showSignoutPrompt = false;
            }

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();
            }

            // no external signout supported for now (see \Quickstart\Account\AccountController.cs TriggerExternalSignout)
            return Ok(new
            {
                showSignoutPrompt,
                ClientName = string.IsNullOrEmpty(context?.ClientName) ? context?.ClientId : context?.ClientName,
                context?.PostLogoutRedirectUri,
                context?.SignOutIFrameUrl,
                logoutId
            });
        }

        [HttpGet]
        [Route("Error")]
        public async Task<IActionResult> Error(string errorId)
        {
            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId);

            if (message != null)
            {
                if (!_environment.IsDevelopment())
                {
                    // only show in development
                    message.ErrorDescription = null;
                }
            }

            return Ok(message);
        }
    }
}
