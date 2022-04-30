using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Interfaces;
using API.Services;
using API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using API.Middleware;

namespace API
{
    public class Startup
    {
        //dependency injection
        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        //Inside this method the ordering is not necessary
        public void ConfigureServices(IServiceCollection services)
        {
            //inside this method there were too many add services - we cut some of them and paste in extension method in ApplicationServiceExtensions
            services.AddApplicationServices(_config); //this customized method in ApplicationServiceExtensions

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIv5", Version = "v1" });
            });
            services.AddCors(); //add CORS to allow http requests use our API

            services.AddIdentityServices(_config); //it is also extension customized method 
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //In this method ordering is VERY IMPORTANT
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //     app.UseDeveloperExceptionPage(); //throw exceptions to the app if needed
                app.UseSwagger(); //this is a tool like a Postman
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
            }

            //instead of UseDeveloperExceptionPage that causes the exceptions - we use our own
            app.UseMiddleware<ExceptionMiddleware>(); //when we get an exception it will be carried out by our ExceptionMiddleware


            app.UseHttpsRedirection();

            app.UseRouting();

            //very important place it after Routing
            app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

            app.UseAuthentication(); //important to place here!
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
