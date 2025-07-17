using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Data;
using Api.Dtos;
using Api.Dtos.User;
using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public AuthServices(IConfiguration configuration, UserManager<User> userManager, ApplicationDbContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
        }

        public string CreateToken(User user)
        {
            var signingKey = _configuration.GetValue<string>("JWT:SigningKey")
                ?? throw new InvalidOperationException("JWT:SigningKey não configurado.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var roles = _userManager.GetRolesAsync(user).Result;
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public async Task<EditedUserDto?> EditUserAsync(string userId, EditedUserDto editedUserDto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new InvalidOperationException("Usuário não encontrado.");
            }

            if (!string.IsNullOrEmpty(editedUserDto.FullName))
            {
                user.FullName = editedUserDto.FullName;
            }
            if (!string.IsNullOrEmpty(editedUserDto.Email))
            {
                user.Email = editedUserDto.Email;
            }
            if (!string.IsNullOrEmpty(editedUserDto.UserName))
            {
                var userNameExists = await _userManager.Users.AnyAsync(u => u.UserName == editedUserDto.UserName && u.Id != userId);
                if (userNameExists)
                {
                    throw new InvalidOperationException("Nome de usuário já está em uso.");
                }
                user.UserName = editedUserDto.UserName;
            }
            if (!string.IsNullOrEmpty(editedUserDto.Password))
            {
                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, editedUserDto.Password);
            }
            if (editedUserDto.Age.HasValue)
            {
                user.Age = editedUserDto.Age.Value;
            }
            if (!string.IsNullOrEmpty(editedUserDto.About))
            {
                user.About = editedUserDto.About;
            }

            var result = _userManager.UpdateAsync(user).Result;
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Erro ao atualizar o usuário.");
            }

            return new EditedUserDto
            {
                UserName = user.UserName,
                Email = user.Email,
                About = user.About,
                Age = user.Age,
                Password = "******", 
                FullName = user.FullName
            };
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<UserDto?> LoginAsync(LoginUserDto loginUserDto)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(x => x.UserName == loginUserDto.UserName);

            if (user == null)
                throw new InvalidOperationException("Usuário não encontrado.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, loginUserDto.Password);
            if (!passwordValid)
                throw new InvalidOperationException("Senha inválida.");

            var accessToken = CreateToken(user);
            var refreshTokenValue = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenValue,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                IsRevoked = false,
                UserId = user.Id
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Token = accessToken,
                RefreshToken = refreshTokenValue
            };
        }

        public async Task<UserDto?> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);
            if (storedToken == null || storedToken.Expires < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Refresh token inválido.");
            }
            var user = storedToken.User;
            if (user == null)
            {
                throw new InvalidOperationException("Usuario não pode ser nulo.");
            }
            storedToken.IsRevoked = true;
            var newRefreshToken = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            var accessToken = CreateToken(user);

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Token = accessToken,
                RefreshToken = newRefreshToken.Token
            };

        }

        public async Task<UserDto?> RegisterAsync(RegisterUserDto registerUserDto)
        {
            var userExists = await _userManager.FindByNameAsync(registerUserDto.UserName);
            if (userExists != null)
                throw new InvalidOperationException("Nome de usuário já existe.");

            var user = new User
            {
                FullName = registerUserDto.FullName,
                UserName = registerUserDto.UserName,
                Email = registerUserDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerUserDto.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException("Erro ao tentar criar o usuário.");

            await _userManager.AddToRoleAsync(user, "Reader");

            var refreshToken = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                IsRevoked = false,
                UserId = user.Id
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Token = CreateToken(user),
                RefreshToken = refreshToken.Token
            };
        }
    }
}
