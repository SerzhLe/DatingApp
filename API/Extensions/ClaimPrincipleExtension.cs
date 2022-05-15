using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class ClaimPrincipleExtension
    {
        public static string GetCurrentUserName(this ClaimsPrincipal claims)
        {
            return claims.FindFirst(ClaimTypes.Name)?.Value; //represent ClaimNames.UniqueName
        }

        public static int GetUserId(this ClaimsPrincipal claims)
        {
            return int.Parse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}