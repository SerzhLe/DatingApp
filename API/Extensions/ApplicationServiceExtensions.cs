using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            //!!!we use add services if we need to use this classes as dependency injection!!!


            //we add configurations and strongly typed keys in cloudinary settings - we need configure this in that way
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            //due to its configuration - we may get access to all properties in appsettings.json via class CloudinarySettings

            services.AddScoped<ITokenService, TokenService>(); //add specific service and it disposes when specific http request ends
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ILikesRepository, LikesRepository>();
            services.AddScoped<LogUserActivity>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}