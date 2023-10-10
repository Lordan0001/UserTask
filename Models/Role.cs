using System.ComponentModel.DataAnnotations;

namespace UserTask.Models
{
    /// <summary>
    /// Represents a role entity.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Gets or sets the unique identifier for the role.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of user roles associated with the role.
        /// </summary>
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
