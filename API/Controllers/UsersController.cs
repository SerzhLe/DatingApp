using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class UsersController : BaseApiController
    {
        //dependency injection
        private readonly DataContext _context;
        public UsersController(DataContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetAllUsers() //https://localhost:5001/api/users
        {
            //when calling database - ALWAYS use asynchronous
            return await _context.Users.ToListAsync();
        }

        //api/users/3 where 3 - id in our case
        [Authorize] //user endpoint protected - to get this method user must be authorized
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id) //https://localhost:5001/api/users/2
        {
            return await _context.Users.SingleAsync(user => user.Id == id); //or you can use - Find(id) 
        }
    }
}