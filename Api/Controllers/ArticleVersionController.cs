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

    [Route("api/article-version")]
    [ApiController]
    public class ArticleVersionController : ControllerBase
    {
        private readonly IArticleVersionServices _articleService;

        public ArticleVersionController(IArticleVersionServices articleService)
        {
            _articleService = articleService;
        }


        [HttpGet("{articleId}")]
        public async Task<IActionResult> GetArticleVersions(string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
            {
                return BadRequest("ID do artigo não pode estar vazio.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Usuário não foi encontrado.");
            }

            var versions = await _articleService.GetArticleVersionsAsync(articleId, userIdClaim.Value);
            if (versions == null || !versions.Any())
            {
                return NotFound("Nenhuma versão encontrada para este artigo.");
            }
            return Ok(versions);
        }

        [Authorize]
        [HttpDelete("delete/{articleId}/{versionNumber}")]
        public async Task<IActionResult> DeleteArticleVersion(Guid articleId, int versionNumber)
        {
            if (articleId == Guid.Empty || versionNumber <= 0)
            {
                return BadRequest("ID do artigo ou número da versão inválidos.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Usuário não foi encontrado.");
            }

            try
            {
                var result = await _articleService.DeleteArticleVersionAsync(articleId, versionNumber, userIdClaim.Value);
                if (!result)
                {
                    return NotFound("Versão não encontrada ou usuário não tem permissão para deletar.");
                }
                return Ok("Versão deletada com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("restore/{articleId}/{versionNumber}")]
        public async Task<IActionResult> RestoreArticleVersion(Guid articleId, int versionNumber)
        {
            if (articleId == Guid.Empty || versionNumber <= 0)
            {
                return BadRequest("ID do artigo ou número da versão inválidos.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Usuário não foi encontrado.");
            }

            try
            {
                var result = await _articleService.RestoreArticleVersionAsync(articleId, versionNumber, userIdClaim.Value);
                if (!result)
                {
                    return NotFound("Versão não encontrada ou usuário não tem permissão para restaurar.");
                }
                return Ok("Versão restaurada com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}