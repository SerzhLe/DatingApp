using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key; //one key is used for both encryption and decryption electronic information
        //it is used with JWT - json web token. This key does not leave the server
        private readonly UserManager<AppUser> _userManager;
        public TokenService(IConfiguration config, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])); //config in appsettings.Development file
        }

        //for implementation requires NuGet package  System.IdentityModel.Tokens.Jwt
        public async Task<string> CreateToken(AppUser user)
        {
            //we need also to put roles in token - token is safe place to save the roles, user cannot modify their roles
            //ONLY if you know secret key - ONLY then you can modify token
            var claims = new List<Claim>() {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),

            }; //all users properties that token should store

            var roles = await _userManager.GetRolesAsync(user);

            //we use ClaimTypes.Role because JwtRegisteredClaimNames does not have role type
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor() //all users properties (claims) + dates and signature
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}