using Api.Data;
using Api.Models;

namespace Services
{
    public class CategoryDataSeeder
    {
        private readonly ApplicationDbContext _context;
        public CategoryDataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public void SeedCategory()
        {
            if (!_context.Categories.Any())
            {
                var categories = new List<Category>
            {
                new Category { Name = "Technology", Slug = "technology" },
                new Category { Name = "Programming", Slug = "programming" },
                new Category { Name = "Artificial Intelligence", Slug = "artificial-intelligence" },
                new Category { Name = "Machine Learning", Slug = "machine-learning" },
                new Category { Name = "Cybersecurity", Slug = "cybersecurity" },
                new Category { Name = "Blockchain", Slug = "blockchain" },
                new Category { Name = "Software Engineering", Slug = "software-engineering" },
                new Category { Name = "Web Development", Slug = "web-development" },
                new Category { Name = "Mobile Development", Slug = "mobile-development" },
                new Category { Name = "Cloud Computing", Slug = "cloud-computing" },
                new Category { Name = "DevOps", Slug = "devops" },
                new Category { Name = "Data Science", Slug = "data-science" },
                new Category { Name = "Big Data", Slug = "big-data" },
                new Category { Name = "Databases", Slug = "databases" },
                new Category { Name = "Frontend", Slug = "frontend" },
                new Category { Name = "Backend", Slug = "backend" },
                new Category { Name = "User Experience", Slug = "user-experience" },
                new Category { Name = "Design", Slug = "design" },
                new Category { Name = "Game Development", Slug = "game-development" },
                new Category { Name = "Virtual Reality", Slug = "virtual-reality" },
                new Category { Name = "Augmented Reality", Slug = "augmented-reality" },
                new Category { Name = "Productivity", Slug = "productivity" },
                new Category { Name = "Startups", Slug = "startups" },
                new Category { Name = "Entrepreneurship", Slug = "entrepreneurship" },
                new Category { Name = "Project Management", Slug = "project-management" },
                new Category { Name = "Marketing", Slug = "marketing" },
                new Category { Name = "SEO", Slug = "seo" },
                new Category { Name = "Finance", Slug = "finance" },
                new Category { Name = "Investments", Slug = "investments" },
                new Category { Name = "Cryptocurrency", Slug = "cryptocurrency" }
            };
                _context.Categories.AddRange(categories);
                _context.SaveChanges();
            }
        }

    }
}