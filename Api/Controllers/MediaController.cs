using System.Security.Claims;
using Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/media")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IMediaServices _mediaServices;

        public MediaController(IMediaServices mediaServices)
        {
            _mediaServices = mediaServices;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMediaFromFile(IFormFile file, string description)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Arquivo inválido.");
            }

            var mediaDto = await _mediaServices.CreateMediaFromFileAsync(file, description);
            return Ok(mediaDto);
        }

        [HttpGet("article/{articleId}")]
        public async Task<IActionResult> GetMediaByArticleId(Guid articleId)
        {
            var mediaItems = await _mediaServices.GetMediaByArticleIdAsync(articleId);
            if (mediaItems == null || !mediaItems.Any())
            {
                return NotFound("Nenhuma mídia encontrada para este artigo.");
            }
            return Ok(mediaItems);
        }

        [Authorize]
        [HttpDelete("{articleId}/{mediaId}")]
        public async Task<IActionResult> DeleteMediaFromArticle(Guid articleId, Guid mediaId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var mediaDeleted = await _mediaServices.DeleteMediaFromArticleAsync(articleId, mediaId, userId);
            if (!mediaDeleted)
            {
                return NotFound("Mídia não encontrada ou usuário não autorizado a deletar esta mídia.");
            }

            return NoContent();
        }

        [Authorize]
        [HttpGet("check-url")]
        public async Task<IActionResult> CheckMediaByUrlAndUserId(string url)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var exists = await _mediaServices.GetMediaByUrlAndUserIdAsync(url, userId);
            return Ok(exists);
        }
    }

}