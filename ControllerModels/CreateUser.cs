using System.ComponentModel.DataAnnotations;

namespace UserTask.ControllerModels
{//чтобы не передавть в контроллер основной класс в котором связанная сущность Role
    public class CreateUser
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string? name { get; set; }
        [Required]

        public int? age { get; set; }

        [Required]
        [EmailAddress]
        public string? email { get; set; }
    }
}
