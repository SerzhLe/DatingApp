using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        //dependency injection
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetAllUsers() //https://localhost:5001/api/users
        {
            //when calling database - ALWAYS use asynchronous

            return Ok(await _userRepository.GetMembersAsync()); //used Ok() to return ActionResult

            //with Include it wil return a user with an object photo that include user object with photo object and that will
            //be cyclical reference - SOLUTION - adding DTO (member - user. And member return PhotoDto)
            //how copy property values from one object to another? - One way - Auto Mapper - download in NuGet AutoMapper with dependency inj
        }

        //api/users/3 where 3 - id in our case
        //user endpoint protected - to get this method user must be authorized
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username) //https://localhost:5001/api/users/2
        {
            return await _userRepository.GetMemberAsync(username);
        }
    }
}