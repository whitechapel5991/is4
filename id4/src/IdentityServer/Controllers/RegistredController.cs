using IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class RegistredController
    {
        //[HttpPost]
        //public async Task<IActionResult> Register([FromBody] SignupRequest request)
        //{
        //    var context = await _interaction.GetAuthorizationContextAsync(request.ReturnUrl);
        //    var user = Config.GetTestUsers()
        //           .FirstOrDefault(usr => usr.Password == request.Password && usr.Claims.Any(x => x.Type == JwtClaimTypes.Email && x.Value == request.Email));

        //    if (user != null)
        //    {
        //        await HttpContext.SignInAsync(user.SubjectId, user.Username);
        //        return new JsonResult(new { RedirectUrl = /*request.ReturnUrl*/"http://localhost:8082/lol", IsOk = true });
        //    }

        //    return Unauthorized();
        //}
    }
}
