using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {

            //we user token base authorization! because our client side is separated from server
            //if we used asp.net MVC - we should use AddIdentity
            services
                .AddIdentityCore<AppUser>(opt =>
                {
                    //all these properties by default are true but I specify them explicitly to know them better
                    opt.Password.RequireNonAlphanumeric = true; //specify is password must contain special characters
                    opt.Password.RequireDigit = true;
                    opt.Password.RequireLowercase = true;
                    opt.Password.RequireUppercase = true;
                })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>() //!!
                .AddSignInManager<SignInManager<AppUser>>()
                .AddRoleValidator<RoleValidator<AppRole>>()
                .AddEntityFrameworkStores<DataContext>(); //creates all needded tables


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) //need middleware for authentication - download package Microsoft.Aspnetcore.Authentication.JwtBearer
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                            ValidateIssuer = false, //issuer - who generates a token
                            ValidateAudience = false //audience - which application can receive this token
                        };

                        //we will add token to queryString because WebSocket cannot user authentication header
                        //when we in Postman give Authentication Bearer - we cannot use it with WebSocket
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"];

                                var path = context.HttpContext.Request.Path;

                                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                                {
                                    context.Token = accessToken; //we implement possibility to push token as query string in request
                                }

                                return Task.CompletedTask;
                            }
                        };
                    });

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            });

            return services;
        }
    }
}