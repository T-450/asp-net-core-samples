using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.NetCore.Identity.Sample.Extensions;
using Asp.NetCore.Identity.Sample.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Asp.NetCore.Identity.Sample.Controllers;

[Route("api/identity")]
public class AuthController : MainController
{
    private readonly AppSettings _appSettings;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
        IOptions<AppSettings> appSettings)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _appSettings = appSettings.Value;
    }

    [HttpPost("create-account")]
    public async ValueTask<ActionResult> SignUp(SignupUser signupUser)
    {
        if (!ModelState.IsValid)
        {
            return CustomResponse(ModelState);
        }

        IdentityUser user = new()
        {
            UserName = signupUser.Email, Email = signupUser.Email, EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, signupUser.Password).ConfigureAwait(false);

        if (result.Succeeded)
        {
            return Ok(await GenerateJwt(signupUser.Email).ConfigureAwait(false));
        }

        foreach (var identityError in result.Errors)
        {
            AddErrors(identityError.Description);
        }

        return CustomResponse();
    }

    [HttpPost("login")]
    public async ValueTask<ActionResult> SignIn(SignInUser signInUser)
    {
        if (!ModelState.IsValid)
        {
            return CustomResponse(ModelState);
        }

        var result = await _signInManager
            .PasswordSignInAsync(signInUser.Email, signInUser.Password, true, false)
            .ConfigureAwait(false);

        if (result.Succeeded)
        {
            return Ok(await GenerateJwt(signInUser.Email).ConfigureAwait(false));
        }

        if (result.IsLockedOut)
        {
            AddErrors("User is temporarily blocked.");
            return CustomResponse();
        }

        if (result.IsNotAllowed)
        {
            AddErrors("User is not allowed to sign-in..");
            return CustomResponse();
        }

        if (result.RequiresTwoFactor)
        {
            AddErrors("Attempting to sign-in requires two factor authentication");
            return CustomResponse();
        }

        AddErrors("Invalid Username or Password.");
        return CustomResponse();
    }

    private async ValueTask<UserSignInResponse> GenerateJwt(string email)
    {
        var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
        var claims = await _userManager.GetClaimsAsync(user).ConfigureAwait(false);
        var userRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

        var claimsList = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Nbf, new DateTime().ToUnixEpochDate(DateTime.UtcNow).ToString()),
            new(JwtRegisteredClaimNames.Nbf, new DateTime().ToUnixEpochDate(DateTime.UtcNow).ToString()),
            new(JwtRegisteredClaimNames.Nbf, new DateTime().ToUnixEpochDate(DateTime.UtcNow).ToString(),
                ClaimValueTypes.Integer64)
        };

        var roleClaims = userRoles.Select(role => new Claim("role", role));
        var identityClaims = new ClaimsIdentity();

        claims.AddRange(claimsList);
        claims.AddRange(roleClaims);
        identityClaims.AddClaims(claims);

        var tokenHandler = new JwtSecurityTokenHandler();
        var keyEncoded = Encoding.ASCII.GetBytes(_appSettings.Secret);

        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _appSettings.Issuer,
            Audience = _appSettings.ValidIn,
            Subject = identityClaims,
            Expires = DateTime.UtcNow.AddHours(_appSettings.ExpirationHours),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyEncoded), SecurityAlgorithms.HmacSha256)
        });
        var tokenEncoded = tokenHandler.WriteToken(token);

        UserSignInResponse payload = new()
        {
            AccessToken = tokenEncoded,
            ExpiresIn = TimeSpan.FromHours(_appSettings.ExpirationHours).TotalSeconds,
            UserToken = new UserToken
            {
                Id = user.Id,
                Email = user.Email,
                Claims = claims.Select(c => new UserClaim
                {
                    Type = c.Type, Value = c.Value
                })
            }
        };

        return payload;
    }
}
