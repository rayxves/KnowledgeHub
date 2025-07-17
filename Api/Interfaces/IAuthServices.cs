using Api.Dtos;
using Api.Dtos.User;
using Api.Models;

namespace Api.Interfaces
{
    public interface IAuthServices
    {
        Task<UserDto?> RegisterAsync(RegisterUserDto registerUserDto);
        Task<UserDto?> LoginAsync(LoginUserDto loginUserDto);
        string CreateToken(User user);
        string GenerateRefreshToken();
        Task<UserDto?> RefreshTokenAsync(string refreshToken);
        Task<EditedUserDto?> EditUserAsync(string userId, EditedUserDto editedUserDto);
    }
}