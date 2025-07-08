using System.ComponentModel.DataAnnotations;

namespace ShopDongY.Models
{
    public class RoleModel
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public virtual ICollection<UserModel> Users { get; set; } = new List<UserModel>();
    }
}  

