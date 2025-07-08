using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopDongY.Models
{
    public class UserModel
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string PhoneNumur { get; set; }
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? Avatar { get; set; } 

        public int? RoleId { get; set; }
        public virtual RoleModel? Role { get; set; }
    }
}
