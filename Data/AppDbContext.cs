using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ProductCatalogApi;

public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(category => category.Name).HasMaxLength(100);
            entity.Property(category => category.Description).HasMaxLength(500);
            entity.Property(category => category.CreatedBy).HasMaxLength(256);
            entity.Property(category => category.LastUpdatedBy).HasMaxLength(256);
        });
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Name).HasMaxLength(100);
            entity.Property(product => product.CreatedBy).HasMaxLength(256);
            entity.Property(product => product.LastUpdatedBy).HasMaxLength(256);
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.Username).HasMaxLength(50);
            entity.Property(user => user.PasswordHash).HasMaxLength(512);
            entity.Property(user => user.Role).HasMaxLength(50);
            entity.Property(user => user.CreatedBy).HasMaxLength(256);
            entity.Property(user => user.LastUpdatedBy).HasMaxLength(256);
        });
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(token => token.TokenHash).HasMaxLength(44);
            entity.Property(token => token.CreatedBy).HasMaxLength(256);
            entity.Property(token => token.LastUpdatedBy).HasMaxLength(256);
        });
        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
        modelBuilder.Entity<Product>().HasOne(p => p.Category).WithMany(c => c.Products)
        .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<User>().HasIndex(user => user.Username).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasOne(rt => rt.User).WithMany()
        .HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.Property(m => m.Reason).HasMaxLength(200);
            entity.Property(m => m.CreatedBy).HasMaxLength(100);
            entity.HasOne(m => m.Product)
            .WithMany()
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(m => m.ProductId);
        });
    }

    public override int SaveChanges()
    {
        return SaveChanges(acceptAllChangesOnSuccess: true);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetAuditFields()
    {
        var now = DateTime.UtcNow;
        var user = _httpContextAccessor?.HttpContext?.User;
        var actor = user?.Identity?.IsAuthenticated == true
            ? user.Identity.Name ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
            : "anonymous";

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.LastUpdate = now;
                entry.Entity.CreatedBy = actor;
                entry.Entity.LastUpdatedBy = actor;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.CreatedAt).IsModified = false;
                entry.Property(entity => entity.CreatedBy).IsModified = false;
                entry.Entity.LastUpdate = now;
                entry.Entity.LastUpdatedBy = actor;
            }
        }
    }
}