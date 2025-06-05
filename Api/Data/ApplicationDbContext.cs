using Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Article> Articles { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Favorite> Favorites { get; set; } = default!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
        public DbSet<ArticleLike> ArticleLikes { get; set; } = default!;
        public DbSet<CommentLike> CommentLikes { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.ArticleId })
                .IsUnique();

            builder.Entity<ArticleLike>()
                .HasIndex(al => new { al.UserId, al.ArticleId })
                .IsUnique();

            builder.Entity<CommentLike>()
                .HasIndex(cl => new { cl.UserId, cl.CommentId })
                .IsUnique();

            builder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

        }

    }
}