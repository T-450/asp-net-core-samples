using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace WebApiFundamentals.Controllers;

[Route("api/authentication")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly string _authSecret;
    private readonly IConfiguration _configuration;

    public AuthenticationController(IConfiguration configuration)
    {
        _configuration = configuration ??
                         throw new ArgumentNullException(nameof(configuration));
        _authSecret = _configuration["Authentication:SecretForKey"];
    }

    [HttpPost("authenticate")]
    public ActionResult<string> Authenticate(
        AuthenticationRequestBody authenticationRequestBody)
    {
        // Step 1: validate the username/password
        var user = ValidateUserCredentials(
            authenticationRequestBody.UserName,
            authenticationRequestBody.Password);

        if (user == null)
        {
            return Unauthorized();
        }

        // Step 2: create a token
        SymmetricSecurityKey securityKey = new(Encoding.ASCII.GetBytes(_authSecret));
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        var claimsForToken = new List<Claim>
        {
            new("sub", user.UserId.ToString(CultureInfo.InvariantCulture)),
            new("given_name", user.FirstName),
            new("family_name", user.LastName),
            new("city", user.City)
        };

        JwtSecurityToken jwtSecurityToken = new(
            _configuration["Authentication:Issuer"],
            _configuration["Authentication:Audience"],
            claimsForToken,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            signingCredentials);

        var tokenToReturn = new JwtSecurityTokenHandler()
            .WriteToken(jwtSecurityToken);

        return Ok(tokenToReturn);
    }

    private CityInfoUser? ValidateUserCredentials(string? userName, string? password)
    {
        // we don't have a user DB or table.  If you have, check the passed-through
        // username/password against what's stored in the database.
        //
        // Here we assume the credentials are valid

        // return a new CityInfoUser (values would normally come from your user DB/table)
        return new CityInfoUser(
            1,
            userName ?? "",
            "Kevin",
            "Dockx",
            "Antwerp");
    }

    // we won't use this outside of this class, so we can scope it to this namespace
    public class AuthenticationRequestBody
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    private class CityInfoUser
    {
        public CityInfoUser(
            int userId,
            string userName,
            string firstName,
            string lastName,
            string city)
        {
            UserId = userId;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            City = city;
        }

        public int UserId { get; }
        public string UserName { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string City { get; }
    }
}
