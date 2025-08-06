using Sample Company.Domain.Entities;

namespace Sample Company.Domain.Interfaces;

public interface ITodoRepository
{
    Task<IEnumerable<TodoItem>> GetAllAsync();
    Task<TodoItem?> GetByIdAsync(int id);
    Task<TodoItem> CreateAsync(TodoItem todoItem);
    Task<TodoItem> UpdateAsync(TodoItem todoItem);
    Task DeleteAsync(int id);
    Task<IEnumerable<TodoItem>> GetByCompletionStatusAsync(bool isCompleted);
    Task<bool> ExistsAsync(int id);
} 