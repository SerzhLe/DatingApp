using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        //return type ActionResult - normal type of return type methods in Controller
        //If we didn't have [ApiController] attribute - we would need to specify in () of method where we get the information from (e.g. [From body])
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            //we want the username to be unique
            if (await UserExists(registerDto.UserName)) return BadRequest("Username already exists"); //ActionResult return any of HTTP status codes

            using var hmac = new HMACSHA512(); //creates a hash object. We use 'using' because this class has Dispose()

            var user = new AppUser()
            {
                UserName = registerDto.UserName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)), //we generate byte[] of password string and compute hash value
                PasswordSalt = hmac.Key //byte[]
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto() { UserName = registerDto.UserName, Token = _tokenService.CreateToken(user) };
        }
        //in this method we use two parameters and return user object (with its id and password) - Not a good realization!
        //that's why we need to use DTO pattern - Data transfer object
        //that pattern assumes creating another class and 

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == loginDto.UserName);

            if (user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt); //every time a hash for definite string will be not the same
            //in order to generate the same hash for the string password as user had while registration - we neen to pass here the secret key
            //It is Password Salt that tells the HMACSHA512 how to encrypt the string pass BECAUSE Salt equals a key of this class

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return new UserDto()
            {
                UserName = loginDto.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.SingleOrDefault(photo => photo.IsMain)?.Url //will be lazy loaded
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(user => user.UserName == username);
        }
    }
}