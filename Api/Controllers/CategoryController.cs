
using Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{


    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryServices _categoryServices;

        public CategoryController(ICategoryServices categoryServices)
        {
            _categoryServices = categoryServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryServices.GetCategoriesAsync();
            if (categories == null || !categories.Any())
            {
                return NotFound("Nenhuma categoria encontrada.");
            }

            return Ok(categories);
        }

        [HttpGet("get-articles-by-user/{slug}")]
        public async Task<IActionResult> GetArticleByCategoryAndUser(string slug, string userId)
        {
            var articles = await _categoryServices.GetArticleByCategoryAndUserIdAsync(slug, userId);
            if (articles == null)
            {
                return NotFound("Nenhum artigo encontrado para essa categoria.");
            }
            return Ok(articles);
        }

        [HttpGet("get-articles/{slug}")]
        public async Task<IActionResult> GetArticlesByCategory(string slug)
        {
            var articles = await _categoryServices.GetArticlesByCategoryAsync(slug);
            if (articles == null || !articles.Any())
            {
                return NotFound("Nenhum artigo encontrado para essa categoria.");
            }
            return Ok(articles);
        }
    }
}