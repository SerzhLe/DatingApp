using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] //we specify how to call the api on site
    public class UsersController : ControllerBase
    {
        //dependency injection
        private readonly DataContext _context;
        public UsersController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetAllUsers() //https://localhost:5001/api/users
        {
            //when calling database - ALWAYS use asynchronous
            return await _context.Users.ToListAsync();
        }

        //api/users/3 where 3 - id in our case
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id) //https://localhost:5001/api/users/2
        {
            return await _context.Users.SingleAsync(user => user.Id == id); //or you can use - Find(id) 
        }
    }
}