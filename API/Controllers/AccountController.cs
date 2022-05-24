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
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(ITokenService tokenService, IMapper mapper,
            UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        //return type ActionResult - normal type of return type methods in Controller
        //If we didn't have [ApiController] attribute - we would need to specify in () of method where we get the information from (e.g. [From body])
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            //we want the username to be unique
            if (await UserExists(registerDto.UserName)) return BadRequest("Username already exists");
            //ActionResult return any of HTTP status codes

            var user = _mapper.Map<RegisterDto, AppUser>(registerDto);

            //IMPORTANT! With implementing Asp.Net Identity we DO NOT NEED MORE password hashing!
            //using var hmac = new HMACSHA512(); //creates a hash object. We use 'using' because this class has Dispose()
            //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)); //we generate byte[] of password string and compute hash value
            //user.PasswordSalt = hmac.Key; //byte[]

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            //creates user, hash password and save changes

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            return new UserDto()
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        //in this method we use two parameters and return user object (with its id and password) - Not a good realization!
        //that's why we need to use DTO pattern - Data transfer object
        //that pattern assumes creating another class and 

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.UserName == loginDto.UserName);
            //we can use Include in userManager

            if (user == null) return Unauthorized("Invalid username");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized();

            //DO NOT USE MORE this custom authentication - we will use Identity 
            //using var hmac = new HMACSHA512(user.PasswordSalt); //every time a hash for definite string will be not the same
            //in order to generate the same hash for the string password as user had while registration - we neen to pass here the secret key
            //It is Password Salt that tells the HMACSHA512 how to encrypt the string pass BECAUSE Salt equals a key of this class
            //var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            //for (int i = 0; i < computedHash.Length; i++)
            //{
            //if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            //}

            return new UserDto()
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.SingleOrDefault(photo => photo.IsMain)?.Url, //eager loaded earlier
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(user => user.UserName == username);
        }
    }
}