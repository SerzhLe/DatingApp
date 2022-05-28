using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Data.Repositories;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            //!!!we use add services if we need to use this classes as dependency injection!!!

            services.AddSingleton<PresenceTracker>();//we create ONE object of presence tracker for all API calls!

            //we add configurations and strongly typed keys in cloudinary settings - we need configure this in that way
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            //due to its configuration - we may get access to all properties in appsettings.json via class CloudinarySettings

            services.AddScoped<ITokenService, TokenService>(); //add specific service and it disposes when specific http request ends
            services.AddScoped<IPhotoService, PhotoService>();

            //instead of all repositories as services we will use unit of work as service
            services.AddScoped<IUnitOfWork, UnitOfWork>();
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