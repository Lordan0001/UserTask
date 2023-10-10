using System.ComponentModel.DataAnnotations;

namespace UserTask.ControllerModels
{
    /// <summary>
    /// Data Transfer Object (DTO) for creating or updating a user role.
    /// </summary>
    public class DTOUserRole
    {
        /// <summary>
        /// Gets or sets the user's ID.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive number")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the role's ID.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "RoleId must be a positive number")]
        public int RoleId { get; set; }
    }
}
