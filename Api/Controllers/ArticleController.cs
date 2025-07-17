namespace Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Api.Data;
    using Api.Models;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Services;
    using System.Security.Claims;
    using Api.Interfaces;
    using Api.Dtos;
    using Microsoft.AspNetCore.Authorization;

    [Route("api/article")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleServices _articleService;

        public ArticleController(IArticleServices articleService)
        {
            _articleService = articleService;
        }


        [HttpGet]
        public async Task<IActionResult> GetArticles()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            userIdClaim ??= new Claim(ClaimTypes.NameIdentifier, "anonymous");
            var articles = await _articleService.GetAllPublicArticlesAsync(userIdClaim.Value);
            if (articles == null || !articles.Any())
            {
                return NotFound("Nenhum artigo encontrado.");
            }
            return Ok(articles);
        }

        [Authorize]
        [HttpGet("by-user")]
        public async Task<IActionResult> GetUserArticles()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Usuário não foi encontrado.");
            }
            var articles = await _articleService.GetMyArticlesAsync(userIdClaim.Value);
            if (articles == null || !articles.Any())
            {
                return NotFound("Nenhum artigo encontrado para esse usuário.");
            }
            return Ok(articles);
        }

        [HttpGet("public-by-user/{userId}")]
        public async Task<IActionResult> GetPublicArticlesByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId não pode estar vazio.");
            }

            var articles = await _articleService.GetPublicArticlesByUserIdAsync(userId);
            if (articles == null || !articles.Any())
            {
                return NotFound("Nenhum artigo público encontrado para esse usuário.");
            }
            return Ok(articles);
        }

        [HttpGet("by-title-and-username")]
        public async Task<IActionResult> GetArticleByTitleAndUsername([FromBody] string username, string title)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(title))
            {
                return BadRequest("Username e title não podem estar vazios.");
            }

            try
            {
                var article = await _articleService.GetArticleByTitleAndUsernameAsync(username, title);
                if (article == null)
                {
                    return NotFound("Nenhum artigo encontrado com o título e usuário fornecidos.");
                }
                return Ok(article);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateArticle([FromBody] CreateArticleDto createArticleDto)
        {

            if (createArticleDto == null)
            {
                return BadRequest("Dados invalidos ou vazios.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Usuário não foi encontrado.");
            }

            var createdArticle = await _articleService.CreateArticleAsync(createArticleDto, userIdClaim.Value);
            if (createdArticle == null)
            {
                return BadRequest("Falha ao criar o artigo.");
            }
            return CreatedAtAction(nameof(GetArticles), createdArticle);
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateArticle([FromBody] UpdateArticleDto updateArticleDto)
        {
            if (updateArticleDto == null)
            {
                return BadRequest("Dados inválidos ou vazios.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Usuário não foi encontrado.");
            }

            var updatedArticle = await _articleService.UpdateArticleAsync(updateArticleDto, userIdClaim.Value);
            if (updatedArticle == null)
            {
                return NotFound("Nenhum artigo encontrado.");
            }
            return Ok(updatedArticle);
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteArticle(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Id do artigo inválido ou vazio.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Usuário não foi encontrado.");
            }

            var isDeleted = await _articleService.DeleteArticleAsync(userIdClaim.Value, id);
            if (!isDeleted)
            {
                return NotFound("Nenhum artigo encontrado ou não pode ser deletado.");
            }
            return NoContent();
        }

        [HttpPost("like/{articleId}")]
        public async Task<IActionResult> LikeArticle(Guid articleId)
        {
            if (articleId == Guid.Empty)
            {
                return BadRequest("ID do artigo inválido.");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não autenticado.");
            }
            try
            {
                var result = await _articleService.LikeArticleAsync(articleId, userId);
                return Ok(new { success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("unlike/{articleId}")]
        public async Task<IActionResult> UnlikeArticle(Guid articleId)
        {
            if (articleId == Guid.Empty)
            {
                return BadRequest("ID do artigo inválido.");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não autenticado.");
            }
            try
            {
                var result = await _articleService.UnlikeArticleAsync(articleId, userId);
                return Ok(new { success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchArticles([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Termo de pesquisa não pode estar vazio.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            userIdClaim ??= new Claim(ClaimTypes.NameIdentifier, "anonymous");
            var articles = await _articleService.GetArticlesBySearchAsync(searchTerm, userIdClaim.Value);
            if (articles == null || !articles.Any())
            {
                return NotFound("Nenhum artigo encontrado com o termo de pesquisa fornecido.");
            }
            return Ok(articles);
        }


    }
}