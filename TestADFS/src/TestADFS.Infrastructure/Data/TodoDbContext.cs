using TestCorp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TestCorp.Infrastructure.Data;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.UpdatedBy).HasMaxLength(255);
            entity.Property(e => e.Priority).HasConversion<int>();

            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.Priority);
        });

        // Seed data
        modelBuilder.Entity<TodoItem>().HasData(
            new TodoItem
            {
                Id = 1,
                Title = "Setup TestCorp Project",
                Description = "Initialize the TestCorp project with proper architecture",
                Priority = Domain.Enums.TodoPriority.High,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TodoItem
            {
                Id = 2,
                Title = "Implement Authentication",
                Description = "Add OAuth and ADFS authentication to the application",
                Priority = Domain.Enums.TodoPriority.Medium,
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            }
        );
    }
} 