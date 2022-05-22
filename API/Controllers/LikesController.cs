using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Extensions;
using API.Entities;
using API.DTOs;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _likesRepository = likesRepository;
            _userRepository = userRepository;
        }

        [HttpPost("{username}")] //...api/likes/username
        public async Task<ActionResult> Addlike(string username)
        {
            var sourceUserId = User.GetUserId();

            var likedUser = await _userRepository.GetUserByUsernameAsync(username);

            var sourceUser = await _likesRepository.GetUserWithLikesAsync(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await _likesRepository.GetUserLikeAsync(sourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("You already liked this user!");

            userLike = new UserLike()
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpDelete("{username}")] //...api/likes/username
        public async Task<ActionResult> DeleteLike(string username)
        {
            var sourceUserId = User.GetUserId();

            var unlikedUser = await _userRepository.GetUserByUsernameAsync(username);

            var sourceUser = await _likesRepository.GetUserWithLikesAsync(sourceUserId);

            if (unlikedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You cannot unlike yourself");

            var userLike = await _likesRepository.GetUserLikeAsync(sourceUserId, unlikedUser.Id);

            if (userLike == null) return BadRequest("You have not liked this user yet!");

            sourceUser.LikedUsers.Remove(userLike);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to unlike user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();

            var likers = await _likesRepository.GetUserLikesAsync(likesParams);

            Response.AddPaginationHeader(likers.CurrentPage, likers.PageSize,
                likers.TotalCount, likers.TotalPages);

            return Ok(likers);
        }
    }
}