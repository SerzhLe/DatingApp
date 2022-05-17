using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLikeAsync(int sourceUserId, int likedUserId);

        Task<AppUser> GetUserWithLikesAsync(int userId);

        Task<PagedList<LikeDto>> GetUserLikesAsync(LikesParams likesParams);
        //method will return likes of a user or likes that user has based on predicate value

        Task<IEnumerable<UserLike>> GetAllUserLikesAsync(string predicate, int userId);
    }
}