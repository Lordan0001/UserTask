using System.ComponentModel.DataAnnotations;
using System.Data;

namespace UserTask.Models
{
    public class UserRole
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required(ErrorMessage = "RoleId is required")]
        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
