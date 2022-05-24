using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users
            .Where(u => u.UserName == username)
            //with ProjectTo we do not need specify Include() method
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider) //kind of optimization - we do not need to load all users than load them in memory and copy in members
            .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();
            query = query.Where(user => user.Gender == userParams.Gender && user.UserName != userParams.CurrentUserName);

            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
            var minDob = DateTime.Today.AddYears(-(userParams.MaxAge) - 1).AddDays(1);

            query = query.Where(user => user.DateOfBirth >= minDob && user.DateOfBirth <= maxDob);

            if (userParams.OrderIsDescending)
            {
                query = userParams.OrderBy switch
                {
                    "created" => query.OrderByDescending(u => u.Created),
                    _ => query.OrderByDescending(u => u.LastActive)
                };
            }

            else
            {
                query = userParams.OrderBy switch
                {
                    "created" => query.OrderBy(u => u.Created),
                    _ => query.OrderBy(u => u.LastActive)
                };
            }


            //these returned entities will not be tracked by EF - AsNoTracking()
            //this is kind of optimization - use method when we need readonly list of entities

            return await PagedList<MemberDto>.CreateAsync(query
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking(), userParams.PageNumber, userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(p => p.Photos).ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0; //if changes are saved, method returns number of changes
        }

        public void UpdateProfile(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}