using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserLike> GetUserLikeAsync(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikesAsync(LikesParams likesParams)
        {
            var users = _context.Users.AsQueryable();
            var likes = _context.Likes.AsQueryable();


            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.LikedUser);
            }

            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }

            users = users.OrderBy(u => u.UserName.ToLower());

            var likers = users.Select(u => new LikeDto()
            {
                Id = u.Id,
                UserName = u.UserName,
                Age = u.DateOfBirth.CalculateAge(),
                KnownAs = u.KnownAs,
                PhotoUrl = u.Photos.SingleOrDefault(p => p.IsMain).Url,
                City = u.City
            });

            return await PagedList<LikeDto>.CreateAsync(likers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikesAsync(int userId)
        {
            return await _context.Users.Include(u => u.LikedUsers).SingleOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IEnumerable<UserLike>> GetAllUserLikesAsync(string predicate, int userId)
        {
            var likes = _context.Likes.AsQueryable();

            if (predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == userId);
            }

            if (predicate == "likedBy")
            {
                likes = likes.Where(like => like.LikedUserId == userId);
            }

            return await likes.ToListAsync();
        }
    }
}