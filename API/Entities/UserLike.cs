using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities
{
    public class UserLike
    {
        //many to many relationships 
        //it is just a join table that has two columns with id of appUsers
        public AppUser SourceUser { get; set; } //user that likes others users
        public int SourceUserId { get; set; }
        public AppUser LikedUser { get; set; }
        public int LikedUserId { get; set; }
    }
}