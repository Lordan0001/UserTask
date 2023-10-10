using System.ComponentModel.DataAnnotations;

namespace UserTask.Models
{
    /// <summary>
    /// Represents a user-role relationship entity.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user-role relationship.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user associated with this user-role relationship.
        /// </summary>
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user associated with this user-role relationship.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Gets or sets the ID of the role associated with this user-role relationship.
        /// </summary>
        [Required(ErrorMessage = "RoleId is required")]
        public int RoleId { get; set; }

        /// <summary>
        /// Gets or sets the role associated with this user-role relationship.
        /// </summary>
        public Role? Role { get; set; }
    }
}
