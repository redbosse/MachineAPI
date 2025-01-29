using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using MachineAPI.Model;
using MongoDB.Bson;
using Serilog;

namespace MachineAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IPersonDataBase _personDataBase;

        public AuthorizationController(IPersonDataBase personDataBase)
        {
            _personDataBase = personDataBase;
        }

        [HttpPost("/register/")]
        public string RegisterUser(string username, string password, Role role)
        {
            var user = new Person { Username = username, Password = password, Role = role };

            user.Id = ObjectId.GenerateNewId();

            if (!_personDataBase.AddNewUser(user))
            {
                Log.Error("There is already a user with that name");

                return "There is already a user with that name";
            }

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Username) };

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        [HttpPost("/login/")]
        public string Login(string Username, string password)
        {
            bool isValidate = _personDataBase.ValidateUser(new Person { Username = Username });

            if (!isValidate)
            {
                return "Authorization failed";
            }

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, Username) };

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            if (_personDataBase.ValidatePassword(new Person { Username = Username, Password = password }))
                return encodedJwt;

            return "Authorization failed";
        }

        [HttpPost("/getAllPerson/")]
        public string GetAllPerson()
        {
            return _personDataBase.GetUserlist();
        }
    }
}