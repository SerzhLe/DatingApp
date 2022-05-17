using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;

namespace API.DTOs
{
    public class MemberDto
    {
        //we copy properties from AppUser to MemberDto using AutoMapper
        //it defines all properties with the same name and copy its values
        //Age - because we use GetAge - mapper looks at methods with name Get and it run this methods and copy values to definite properties
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PhotoUrl { get; set; } //this property does not exist in original User class - go! to mapper class for configuration
        public int Age { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public bool IsLiked { get; set; } = false;
        public ICollection<PhotoDto> Photos { get; set; }
    }
}