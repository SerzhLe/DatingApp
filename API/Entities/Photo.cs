using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table("Photos")] //we will not work with photos as a table in db, just want to have this table
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        public string PublicId { get; set; } //for photo storage solution in future!
        public AppUser AppUser { get; set; }
        public int AppUserId { get; set; }
    }
}