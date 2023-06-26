using System.ComponentModel.DataAnnotations;

namespace Authentication.Models
{
    public class UserEntity
    {
        [Key]
        public int Id { get; set; }
        public string? email { get; set; }
        public string? username { get; set; }
        public string? hashedPassword { get; set; } 

    }
}
