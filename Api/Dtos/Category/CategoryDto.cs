using System.ComponentModel.DataAnnotations;

namespace Api.Dtos
{
    public class CategoryDto
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;
   
    }
}
