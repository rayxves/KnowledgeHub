
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

        [HttpGet("get-articles/{slug}")]
        public async Task<IActionResult> GetArticleByCategory(string slug, string userId)
        {
            var article = await _categoryServices.GetArticleByCategoryAsync(slug, userId);
            if (article == null)
            {
                return NotFound("Nenhum artigo encontrado para essa categoria.");
            }
            return Ok(article);
        }
    }
}