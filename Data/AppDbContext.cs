using Microsoft.EntityFrameworkCore;

namespace ProductCatalogApi;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
        modelBuilder.Entity<Product>().HasOne(p => p.Category).WithMany(c => c.Products)
        .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<User>().HasIndex(user => user.Username).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasOne(rt => rt.User).WithMany()
        .HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}