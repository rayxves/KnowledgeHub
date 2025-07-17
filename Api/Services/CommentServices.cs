using Api.Data;
using Api.Dtos;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class CommentServices : ICommentServices
    {
        private readonly ApplicationDbContext _context;

        public CommentServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GetCommentDto> CreateCommentAsync(CreateCommentDto createCommentDto, string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("Ususário não encontrado.");
            }

            var article = await _context.Articles.FindAsync(createCommentDto.ArticleId);
            if (article == null)
            {
                throw new InvalidOperationException("Artigo não encontrado.");
            }

            var comment = new Comment
            {
                Text = createCommentDto.Text,
                ArticleId = createCommentDto.ArticleId,
                Article = article,
                UserId = userId,
                User = user,
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = createCommentDto.ParentCommentId,
                Replies = new List<Comment>(),
                Likes = new List<CommentLike>()
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return new GetCommentDto
            {
                Id = comment.Id,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                CreatedBy = user.UserName,
                LikesCount = 0,
                UserLiked = false,
                Replies = new List<GetCommentDto>(),
                ParentCommentId = comment.ParentCommentId,

            };


        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, string userId)
        {

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);
            if (comment == null)
            {
                throw new InvalidOperationException("Commentário não encontrado ou você não tem permissão para excluí-lo.");
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GetCommentDto> GetCommentByUserIdAndTextAsync(string userId, string text)
        {

            var comment = await _context.Comments
                 .Include(c => c.Likes)
                 .Where(c => c.UserId == userId && c.Text.Contains(text))
                 .Select(c => new GetCommentDto
                 {
                     Id = c.Id,
                     Text = c.Text,
                     CreatedAt = c.CreatedAt,
                     CreatedBy = c.User.UserName,
                     LikesCount = c.Likes.Count,
                     UserLiked = c.Likes.Any(l => l.UserId == userId),
                     Replies = c.Replies.Any() ? c.Replies.Select(r => new GetCommentDto
                     {
                         Id = r.Id,
                         Text = r.Text,
                         CreatedAt = r.CreatedAt,
                         CreatedBy = r.User.UserName,
                         LikesCount = r.Likes.Count,
                         UserLiked = r.Likes.Any(l => l.UserId == userId),
                     }).ToList() : new List<GetCommentDto>(),
                     ParentCommentId = c.ParentCommentId != null ? c.ParentCommentId : null
                 }).FirstOrDefaultAsync();

            return comment ?? throw new InvalidOperationException("Comment not found");
        }

        public async Task<IEnumerable<GetCommentDto>> GetCommentsByArticleIdAsync(Guid articleId, string userId)
        {
            var comments = await _context.Comments
                 .Include(c => c.Likes)
                 .Where(c => c.ArticleId == articleId && c.ParentCommentId == null)
                 .Select(c => new GetCommentDto
                 {
                     Id = c.Id,
                     Text = c.Text,
                     CreatedAt = c.CreatedAt,
                     CreatedBy = c.User.UserName,
                     LikesCount = c.Likes.Count,
                     UserLiked = c.Likes.Any(l => l.UserId == userId),
                     Replies = c.Replies.Any() ? c.Replies.Select(r => new GetCommentDto
                     {
                         Id = r.Id,
                         Text = r.Text,
                         CreatedAt = r.CreatedAt,
                         CreatedBy = r.User.UserName,
                         LikesCount = r.Likes.Count,
                         UserLiked = r.Likes.Any(l => l.UserId == userId),
                     }).ToList() : new List<GetCommentDto>(),
                     ParentCommentId = c.ParentCommentId != null ? c.ParentCommentId : null
                 }).ToListAsync();

            return comments;
        }

        public async Task<IEnumerable<GetCommentDto>> GetCommentsByParentCommentIdAsync(Guid parentCommentId, string userId)
        {
            var comments = await _context.Comments
                .Include(c => c.Likes)
                .Where(c => c.ParentCommentId == parentCommentId)
                .Select(c => new GetCommentDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.User.UserName,
                    LikesCount = c.Likes.Count,
                    UserLiked = c.Likes.Any(l => l.UserId == userId),
                    Replies = c.Replies.Any() ? c.Replies.Select(r => new GetCommentDto
                    {
                        Id = r.Id,
                        Text = r.Text,
                        CreatedAt = r.CreatedAt,
                        CreatedBy = r.User.UserName,
                        LikesCount = r.Likes.Count,
                        UserLiked = r.Likes.Any(l => l.UserId == userId),
                    }).ToList() : new List<GetCommentDto>(),
                    ParentCommentId = c.ParentCommentId != null ? c.ParentCommentId : null
                }).ToListAsync();

            return comments;
        }

        public async Task<IEnumerable<GetCommentDto>> GetCommentsByUserIdAndArticleIdAsync(string userId, Guid articleId)
        {

            var article = await _context.Articles.FindAsync(articleId);
            if (article == null)
            {
                throw new InvalidOperationException("Article not found");
            }
            return await _context.Comments
                .Include(c => c.Likes)
                .Where(c => c.UserId == userId && c.ArticleId == articleId && c.ParentCommentId == null)
                .Select(c => new GetCommentDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.User.UserName,
                    LikesCount = c.Likes.Count,
                    UserLiked = c.Likes.Any(l => l.UserId == userId),
                    Replies = c.Replies.Any() ? c.Replies.Select(r => new GetCommentDto
                    {
                        Id = r.Id,
                        Text = r.Text,
                        CreatedAt = r.CreatedAt,
                        CreatedBy = r.User.UserName,
                        LikesCount = r.Likes.Count,
                        UserLiked = r.Likes.Any(l => l.UserId == userId),
                    }).ToList() : new List<GetCommentDto>(),
                    ParentCommentId = c.ParentCommentId != null ? c.ParentCommentId : null
                }).ToListAsync();
        }
        public async Task<bool> LikeCommentAsync(Guid commentId, string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("Usuário não encontrado.");
            }

            var comment = await _context.Comments
                .Include(c => c.Likes)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                throw new InvalidOperationException("Comentário não encontrado.");
            }

            if (comment.Likes.Any(l => l.UserId == userId))
            {
                throw new InvalidOperationException("O usuário já curtiu este comentário.");
            }

            var commentLike = _context.CommentLikes.Add(new CommentLike
            {
                UserId = userId,
                CommentId = commentId,
                LikedAt = DateTime.UtcNow,
                User = user,
                Comment = comment
            });

            comment.Likes.Add(commentLike.Entity);

            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnlikeCommentAsync(Guid commentId, string userId)
        {
            var comment = await _context.Comments
                .Include(c => c.Likes)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                throw new InvalidOperationException("Comentário não encontrado.");
            }

            var like = comment.Likes.FirstOrDefault(l => l.UserId == userId);
            if (like == null)
            {
                throw new InvalidOperationException("O usuário ainda não curtiu este comentário.");
            }

            comment.Likes.Remove(like);
            _context.CommentLikes.Remove(like);
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<GetCommentDto> UpdateCommentAsync(UpdateCommentDto updateCommentDto, string userId)
        {

            var comment = await _context.Comments.Include(c => c.Likes).FirstOrDefaultAsync(c => c.Id == updateCommentDto.Id && c.UserId == userId);
            if (comment == null)
            {
                throw new InvalidOperationException("Comentário não encontrado ou você não tem permissão para atualizá-lo.");
            }

            comment.Text = updateCommentDto.Text;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            return new GetCommentDto
            {
                Id = comment.Id,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                CreatedBy = comment.User.UserName,
                LikesCount = comment.Likes.Count,
                UserLiked = comment.Likes.Any(l => l.UserId == userId),
                Replies = comment.Replies.Any() ? comment.Replies.Select(r => new GetCommentDto
                {
                    Id = r.Id,
                    Text = r.Text,
                    CreatedAt = r.CreatedAt,
                    CreatedBy = r.User.UserName,
                    LikesCount = r.Likes.Count,
                    UserLiked = r.Likes.Any(l => l.UserId == userId),
                }).ToList() : new List<GetCommentDto>(),
                ParentCommentId = comment.ParentCommentId != null ? comment.ParentCommentId : null

            };
        }
    }
}