namespace Api.Dtos.User
{
    public class EditedUserDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? About { get; set; }
        public int? Age { get; set; }
    }
}
