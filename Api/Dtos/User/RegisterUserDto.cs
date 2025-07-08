using System.ComponentModel.DataAnnotations;

namespace Api.Dtos
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Nome completo é obrigatório.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username é obrigatório.")]
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email é obrigatório.")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Senha é obrigatória.")]
        public string Password { get; set; } = string.Empty;
    }
}