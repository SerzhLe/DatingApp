using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data.EntityConfigurations;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser, AppRole, int,
        IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>> //user, role, key...

    //change dbcontext to identity - donwload nuget - microsoft.aspnetcore.identity.efcore 
    {
        public DataContext(DbContextOptions options)
            : base(options)
        {
        }

        //public DbSet<AppUser> Users { get; set; } REMOVE THIS TABLE because IdentiyDbContext provides it
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); //to avoid possible errors when migrating

            builder.ApplyConfiguration(new RoleConfigurations());

            builder.ApplyConfiguration(new LikeConfiguration());

            builder.ApplyConfiguration(new MessageConfiguration());

        }
    }
}