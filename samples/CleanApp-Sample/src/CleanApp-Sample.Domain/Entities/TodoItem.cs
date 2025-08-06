using Sample Company.Domain.Enums;

namespace Sample Company.Domain.Entities;

public class TodoItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    public DateTime? CompletedAt { get; set; }
    public string? Tags { get; set; }

    public void MarkAsCompleted()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsIncomplete()
    {
        IsCompleted = false;
        CompletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
} 