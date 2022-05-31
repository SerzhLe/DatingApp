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
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {

        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _unitOfWork;
        public UsersController(IUnitOfWork unitOfWork, IMapper mapper,
            IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetAllUsers([FromQuery] UserParams userParams)
        {
            userParams.CurrentUserName = User.GetCurrentUserName();
            var gender = await _unitOfWork.UserRepository.GetUserGender(userParams.CurrentUserName);

            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = gender == "male" ? "female" : "male";


            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

            var likes = await _unitOfWork.LikesRepository.GetAllUserLikesAsync("liked", User.GetUserId());

            foreach (var member in users)
            {
                if (likes.SingleOrDefault(u => u.LikedUserId == member.Id) != null)
                    member.IsLiked = true;
            }

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);

        }

        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var askedUser = await _unitOfWork.UserRepository.GetMemberAsync(username);

            var likes = await _unitOfWork.LikesRepository.GetAllUserLikesAsync("liked", User.GetUserId());

            if (likes.SingleOrDefault(u => u.LikedUserId == askedUser.Id) != null)
                askedUser.IsLiked = true;

            return askedUser;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(User.GetUserId());

            _mapper.Map(memberUpdateDto, user);

            _unitOfWork.UserRepository.UpdateProfile(user);

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("upload-photo")]
        public async Task<ActionResult<PhotoDto>> UploadPhoto(IFormFile file)
        {
            var username = User.GetCurrentUserName();

            var user = await _unitOfWork.UserRepository.GetUserWithPhotosByUsernameAsync(username);

            var result = await _photoService.UploadImageAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo()
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count <= 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if (await _unitOfWork.Complete())
            {
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<Photo, PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var username = User.GetCurrentUserName();

            var user = await _unitOfWork.UserRepository.GetUserWithPhotosByUsernameAsync(username);

            var photoToDelete = user.Photos.SingleOrDefault(p => p.Id == photoId);

            if (photoToDelete == null) return NotFound("Photo not found");
            if (photoToDelete.IsMain) return BadRequest("You cannot delete your main photo");
            if (photoToDelete.PublicId != null)
            {
                var result = await _photoService.DeleteImageAsync(photoToDelete.PublicId);

                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photoToDelete);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete the photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> MakeMainPhoto(int photoId)
        {
            var username = User.GetCurrentUserName();

            var user = await _unitOfWork.UserRepository.GetUserWithPhotosByUsernameAsync(username);

            var photoToMakeMain = user.Photos.SingleOrDefault(photo => photo.Id == photoId);

            if (photoToMakeMain == null) return BadRequest("Photo with that id is not found");
            if (photoToMakeMain.IsMain) return BadRequest("This is already your main photo");

            var photoCurrentMain = user.Photos.SingleOrDefault(photo => photo.IsMain);

            photoCurrentMain.IsMain = false;

            photoToMakeMain.IsMain = true;

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to make photo as main");
        }
    }
}