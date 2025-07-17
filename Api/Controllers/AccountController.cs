using System.Security.Claims;
using Api.Dtos;
using Api.Dtos.User;
using Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        public AccountController(IAuthServices authServices)
        {
            _authServices = authServices;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _authServices.RegisterAsync(registerUserDto);

                Response.Cookies.Append("refreshToken", user.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    // Secure = true, adicionar em prod
                    Expires = DateTime.UtcNow.AddDays(7),
                    SameSite = SameSiteMode.Strict
                });

                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _authServices.LoginAsync(loginUserDto);

                Response.Cookies.Append("refreshToken", user.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    // Secure = true,
                    Expires = DateTime.UtcNow.AddDays(7),
                    SameSite = SameSiteMode.Strict
                });

                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken == null)
                return Unauthorized(new { message = "Refresh token não encontrado." });

            try
            {
                var userDto = await _authServices.RefreshTokenAsync(refreshToken);

                Response.Cookies.Append("refreshToken", userDto.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    // Secure = true,
                    Expires = DateTime.UtcNow.AddDays(7),
                    SameSite = SameSiteMode.Strict
                });

                return Ok(userDto);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("edit")]
        public async Task<IActionResult> EditUser([FromBody] EditedUserDto editedUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não foi encontrado.");
            }
            try
            {
                var updatedUser = await _authServices.EditUserAsync(userId, editedUserDto);

                return Ok(updatedUser);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
