using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Extensions;
using System.Data.Entity;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        //dependency injection
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetAllUsers() //https://localhost:5001/api/users
        {
            //when calling database - ALWAYS use asynchronous

            return Ok(await _userRepository.GetMembersAsync()); //used Ok() to return ActionResult

            //with Include it will return a user with an object photo that include user object with photo object and that will
            //be cyclical reference - SOLUTION - adding DTO (member - user. And member return PhotoDto)
            //how copy property values from one object to another? - One way - Auto Mapper - download in NuGet AutoMapper with dependency inj
        }


        //user endpoint protected - to get this method user must be authorized
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username) //https://localhost:5001/api/users/2
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut] //Put when we change data
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetCurrentUserName(); //we are getting username from claims in token

            var user = await _userRepository.GetUserByUsernameAsync(username);

            _mapper.Map(memberUpdateDto, user); //copy all properties of memberUpdateDto to user

            _userRepository.UpdateProfile(user);

            if (await _userRepository.SaveAllAsync()) return NoContent(); ///this method will be called ONLY if changes are detected
            //because we explicitly UpdateProfile() - we guarantee that changes are made even if there are no changes - it prevents us from 
            //exception when saving changes

            return BadRequest("Failed to update user");
        }

        [HttpPost("upload-photo")]
        public async Task<ActionResult<PhotoDto>> UploadPhoto(IFormFile file)
        {
            var username = User.GetCurrentUserName();

            var user = await _userRepository.GetUserByUsernameAsync(username);

            var result = await _photoService.UploadImageAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo()
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count <= 0) photo.IsMain = true;

            user.Photos.Add(photo);

            //before we were returning 200Status Ok - IT IS NOT APPROPRIATE when you return media file
            //You must return Created - 201 status - when we create a resource on the server
            //Inside this 201 response must be location header - where you can get this resource
            //We need a location header to get photos without user

            if (await _userRepository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<Photo, PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var username = User.GetCurrentUserName();

            var user = await _userRepository.GetUserByUsernameAsync(username);

            var photoToDelete = user.Photos.SingleOrDefault(p => p.Id == photoId);

            if (photoToDelete == null) return NotFound("Photo not found");
            if (photoToDelete.IsMain) return BadRequest("You cannot delete your main photo");
            if (photoToDelete.PublicId != null)
            {
                var result = await _photoService.DeleteImageAsync(photoToDelete.PublicId);

                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photoToDelete);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete the photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> MakeMainPhoto(int photoId)
        {
            var username = User.GetCurrentUserName();

            var user = await _userRepository.GetUserByUsernameAsync(username);

            var photoToMakeMain = user.Photos.SingleOrDefault(photo => photo.Id == photoId);

            if (photoToMakeMain == null) return BadRequest("Photo with that id is not found");
            if (photoToMakeMain.IsMain) return BadRequest("This is already your main photo");

            var photoCurrentMain = user.Photos.SingleOrDefault(photo => photo.IsMain); //because all photos are in memory - in repository we eager load photos

            photoCurrentMain.IsMain = false;

            photoToMakeMain.IsMain = true;

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to make photo as main");
        }
    }
}