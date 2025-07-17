using System.Security.Claims;
using Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/favorites")]
    [ApiController]
    [Authorize]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteServices _favoriteServices;

        public FavoriteController(IFavoriteServices favoriteServices)
        {
            _favoriteServices = favoriteServices;
        }

        [HttpPost("{articleId}")]
        public async Task<IActionResult> AddToFavorites(Guid articleId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não foi encontrado.");
            }

            var result = await _favoriteServices.AddToFavoritesAsync(articleId, userId);
            if (result)
            {
                return Ok("Artigo adicionado aos favoritos.");
            }
            return BadRequest("Erro ao adicionar artigo aos favoritos.");
        }

        [HttpDelete("{articleId}")]
        public async Task<IActionResult> RemoveFromFavorites(Guid articleId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não foi encontrado.");
            }

            var result = await _favoriteServices.RemoveFromFavoritesAsync(articleId, userId);
            if (result)
            {
                return Ok("Artigo removido dos favoritos.");
            }
            return BadRequest("Erro ao remover artigo dos favoritos.");
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não foi encontrado.");
            }

            var favorites = await _favoriteServices.GetFavoriteArticlesAsync(userId);
            if (favorites == null || !favorites.Any())
            {
                return NotFound("Nenhum favorito encontrado.");
            }
            return Ok(favorites);
        }

        
    }
}