using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppRole : IdentityRole<int>
    //this list of user roles - many to many relationships. Each user may have many relationships and each relationship may have may users
    {
        public ICollection<AppUserRole> UserRoles { get; set; }
        
    }
}