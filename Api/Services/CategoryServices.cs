using Api.Data;
using Api.Dtos;
using Api.Interfaces;
using Api.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class CategoryServices : ICategoryServices
    {
        private readonly ApplicationDbContext _context;

        public CategoryServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<GetArticleDto>> GetArticlesByCategoryAsync(string slug)
        {
            var articleTasks = await _context.Articles
                .Where(a => a.Category.Slug == slug)
                .Select(a => a.ToGetArticleDtoAsync(string.Empty))
                .ToListAsync();

            return articleTasks.Any() ? articleTasks : throw new KeyNotFoundException("Nenhum artigo encontrado para essa categoria.");
        }

        public async Task<IEnumerable<GetArticleDto>> GetArticleByCategoryAndUserIdAsync(string slug, string userId)
        {
            var articleTasks = await _context.Articles
                .Where(a => a.Category.Slug == slug && a.UserId == userId)
                .Select(a => a.ToGetArticleDtoAsync(userId))
                .ToListAsync();

            return articleTasks.Any() ? articleTasks : throw new KeyNotFoundException("Nenhum artigo encontrado para essa categoria e usuário.");
        }
    }
}