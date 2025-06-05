namespace Api.Dtos
{
    public class RegisterUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? About { get; set; }
        public int? Age { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}