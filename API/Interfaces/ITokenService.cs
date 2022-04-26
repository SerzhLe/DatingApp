using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;

namespace API.Interfaces
{
    //we create interface for future simple testing
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}