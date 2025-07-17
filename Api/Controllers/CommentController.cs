using System.Security.Claims;
using Api.Dtos;
using Api.Interfaces;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/comment")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentServices _commentServices;

        public CommentController(ICommentServices commentServices)
        {
            _commentServices = commentServices;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto createCommentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não autenticado.");
            }
            try
            {
                var comment = await _commentServices.CreateCommentAsync(createCommentDto, userId);
                return Ok(comment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("like/{commentId}")]
        public async Task<IActionResult> LikeComment(Guid commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não autenticado.");
            }
            try
            {
                var result = await _commentServices.LikeCommentAsync(commentId, userId);
                return Ok(new { success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("unlike/{commentId}")]
        public async Task<IActionResult> UnlikeComment(Guid commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não autenticado.");
            }
            try
            {
                var result = await _commentServices.UnlikeCommentAsync(commentId, userId);
                return Ok(new { success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("by-article/{articleId}")]
        public async Task<IActionResult> GetCommentsByArticleId(Guid articleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (articleId == Guid.Empty)
            {
                return BadRequest("ID do artigo inválido.");
            }

            userId ??= string.Empty;
            var comments = await _commentServices.GetCommentsByArticleIdAsync(articleId, userId);
            if (comments == null || !comments.Any())
            {
                return NotFound("Nenhum comentário encontrado para este artigo.");
            }
            return Ok(comments);
        }

        [HttpGet("by-user/{userId}/{articleId}")]
        public async Task<IActionResult> GetCommentsByUserIdAndArticleId(string userId, Guid articleId)
        {
            if (string.IsNullOrEmpty(userId) || articleId == Guid.Empty)
            {
                return BadRequest("ID do usuário ou do artigo inválido.");
            }
            var comments = await _commentServices.GetCommentsByUserIdAndArticleIdAsync(userId, articleId);
            if (comments == null || !comments.Any())
            {
                return NotFound("Nenhum comentário encontrado para este usuário e artigo.");
            }
            return Ok(comments);
        }

        [Authorize]
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não autenticado.");
            }
            try
            {
                var result = await _commentServices.DeleteCommentAsync(commentId, userId);
                return Ok(new { success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("by-text")]
        public async Task<IActionResult> GetCommentByUserAndText(string text)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não autenticado.");
            }
            var comment = await _commentServices.GetCommentByUserIdAndTextAsync(userId, text);
            if (comment == null)
            {
                return NotFound("Nenhum comentário encontrado para este usuário.");
            }
            return Ok(comment);
        }

        [HttpGet("replies")]
        public async Task<IActionResult> GetRepliesByCommentId(Guid commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Usuário não autenticado.");
            }

            if (commentId == Guid.Empty)
            {
                return BadRequest("ID do comentário inválido.");
            }

            var replies = await _commentServices.GetCommentsByParentCommentIdAsync(commentId, userId);

            if (replies == null || !replies.Any())
            {
                return NotFound("Nenhuma resposta encontrada para este comentário.");
            }
            return Ok(replies);
        }

        

    }
}