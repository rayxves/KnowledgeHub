using System.ComponentModel.DataAnnotations;

namespace Api.Dtos
{
    public class LoginUserDto
    {
        [Required(ErrorMessage = "Username é obrigatório.")]
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Senha é obrigatória.")]
        public string Password { get; set; } = string.Empty;
    }
}