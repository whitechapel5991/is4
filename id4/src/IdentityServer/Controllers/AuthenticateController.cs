using IdentityModel;
using IdentityServer.Models;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[SecurityHeaders]
    [AllowAnonymous]
    public class AuthenticateController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IWebHostEnvironment _environment;
        private readonly SignInManager<IdentityUser> _signInManager;
        //private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;

        public AuthenticateController(
            IIdentityServerInteractionService interaction,
                 IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
                           IEventService events,
            //UserManager<ApplicationUser> userManager,
              SignInManager<IdentityUser> signInManager,


             IWebHostEnvironment environment
            )
        {
            _interaction = interaction;
            _environment = environment;
            _signInManager = signInManager;
            //_userManager = userManager;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody]LoginRequest requestModel)
        {
            var context = await _interaction.GetAuthorizationContextAsync(requestModel.ReturnUrl);

            if (ModelState.IsValid)
            {
                var user = await _signInManager.UserManager.FindByEmailAsync(requestModel.Email);

                // validate username/password against in-memory store
                if (user != null && (await _signInManager.CheckPasswordSignInAsync(user, requestModel.Password, true)) == Microsoft.AspNetCore.Identity.SignInResult.Success)
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

                    // only set explicit expiration here if user chooses "remember me". 
                    // otherwise we rely upon expiration configured in cookie middleware.
                    AuthenticationProperties props = null;
                    if (/*AccountOptions.AllowRememberLogin*/true && requestModel.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(/*AccountOptions.RememberMeLoginDuration*/TimeSpan.FromDays(30))
                        };
                    };

                    // issue authentication cookie with subject ID and username
                    var isuser = new IdentityServerUser(user.Id)
                    {
                        DisplayName = user.UserName
                    };

                    await HttpContext.SignInAsync(isuser, props);

                    if (context != null)
                    {
                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return Redirect(requestModel.ReturnUrl);
                    }

                    await _events.RaiseAsync(new UserLoginFailureEvent(user.UserName, "invalid credentials", clientId: context?.Client.ClientId));
                    ModelState.AddModelError(string.Empty, /*AccountOptions.InvalidCredentialsErrorMessage*/"Invalid username or password");
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
