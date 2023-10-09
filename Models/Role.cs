using System.ComponentModel.DataAnnotations;

namespace UserTask.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }


        public ICollection<UserRole> UserRoles { get; set; }
    }
}
