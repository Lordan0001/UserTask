using System.ComponentModel.DataAnnotations;

namespace UserTask.ControllerModels
{
    /// <summary>
    /// Represents a Data Transfer Object (DTO) for creating or updating a user, while avoiding issues with the related Role entity.
    /// </summary>
    public class DTOUser
    {
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        public string? name { get; set; }

        /// <summary>
        /// Gets or sets the age of the user.
        /// </summary>
        [Required(ErrorMessage = "Age is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Age must be a positive number")]
        public int? age { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? email { get; set; }
    }
}
