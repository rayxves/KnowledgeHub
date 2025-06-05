using Api.Dtos;
using Api.Models;

namespace Api.Interfaces
{
    public interface IAuthServices
    {
        Task<UserDto?> RegisterAsync(RegisterUserDto registerUserDto);
        Task<UserDto?> LoginAsync(LoginUserDto loginUserDto);
        string CreateToken(User user);
        string GenerateRefreshToken();
    }
}