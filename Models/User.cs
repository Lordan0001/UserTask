using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserTask.Models
{
    public class User
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Age is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Age must be a positive number")]
        public int? Age { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [Required]
        [JsonIgnore]
        public ICollection<UserRole>? UserRoles { get; set; }

    }
}
