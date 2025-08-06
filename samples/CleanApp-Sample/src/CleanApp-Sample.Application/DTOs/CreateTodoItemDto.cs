using Sample Company.Domain.Enums;

namespace Sample Company.Application.DTOs;

public class CreateTodoItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    public string? Tags { get; set; }
} 