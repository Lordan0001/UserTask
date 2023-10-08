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
        [Required]
        public string? Name { get; set; }
        [Required]

        public int? Age { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [JsonIgnore]
        public ICollection<UserRole>? UserRoles { get; set; }

    }
}
