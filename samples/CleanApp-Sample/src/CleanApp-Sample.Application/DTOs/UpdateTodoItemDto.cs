using Sample Company.Domain.Enums;

namespace Sample Company.Application.DTOs;

public class UpdateTodoItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public TodoPriority Priority { get; set; }
    public string? Tags { get; set; }
} 