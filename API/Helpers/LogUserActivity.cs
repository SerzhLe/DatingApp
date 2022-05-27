using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    //class is used for updating the info of Last Active time of each user - implementing "action filter"
    //need to use it as a service
    public class LogUserActivity : IAsyncActionFilter
    {
        //add ServiceFilter to BaseApiController
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) //next - is what happens after response execution
        {
            var resultContext = await next();

            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return; //check if user is logged in

            var userId = resultContext.HttpContext.User.GetUserId(); //get userId

            var repo = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();  //get service of user repos

            var user = await repo.GetUserByIdAsync(userId);

            user.LastActive = DateTime.UtcNow;

            await repo.SaveAllAsync();
        }
    }
}