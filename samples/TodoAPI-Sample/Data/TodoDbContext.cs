using Microsoft.EntityFrameworkCore;
using TodoAPI_Sample.Models;

namespace TodoAPI_Sample.Data;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.IsCompleted);
        });

        // Seed data
        modelBuilder.Entity<TodoItem>().HasData(
            new TodoItem
            {
                Id = 1,
                Title = "Sample Todo Item",
                Description = "This is a sample todo item created during database initialization",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
} 