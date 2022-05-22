using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data.EntityConfigurations;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); //to avoid possible errors when migrating

            builder.ApplyConfiguration(new LikeConfiguration());

            builder.ApplyConfiguration(new MessageConfiguration());
        }
    }
}