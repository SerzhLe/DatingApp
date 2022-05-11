using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class ClaimPrincipleExtension
    {
        public static string GetCurrentUserName(this ClaimsPrincipal claims) {
            return claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}