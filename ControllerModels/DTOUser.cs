using System.ComponentModel.DataAnnotations;

namespace UserTask.ControllerModels
{//чтобы не передавать в контроллер основной класс в котором связанная сущность Role
    public class DTOUser
    {
        [Required(ErrorMessage = "Name is required")]
        public string? name { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Age must be a positive number")]
        public int? age { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? email { get; set; }
    }
}
