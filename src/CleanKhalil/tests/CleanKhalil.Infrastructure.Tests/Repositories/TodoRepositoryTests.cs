using CleanKhalil.Domain.Entities;
using CleanKhalil.Domain.Enums;
using CleanKhalil.Infrastructure.Data;
using CleanKhalil.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CleanKhalil.Infrastructure.Tests.Repositories;

public class TodoRepositoryTests : IDisposable
{
    private readonly TodoDbContext _context;
    private readonly TodoRepository _repository;

    public TodoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TodoDbContext(options);
        _repository = new TodoRepository(_context);
        
        // Ensure database is created
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddTodoToDatabase()
    {
        // Arrange
        var todo = new TodoItem
        {
            Title = "Test Todo",
            Description = "Test Description",
            Priority = TodoPriority.High
        };

        // Act
        var result = await _repository.CreateAsync(todo);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("Test Todo");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        var dbTodo = await _context.TodoItems.FindAsync(result.Id);
        dbTodo.Should().NotBeNull();
        dbTodo!.Title.Should().Be("Test Todo");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTodos()
    {
        // Arrange
        var todos = new[]
        {
            new TodoItem { Title = "Todo 1", Priority = TodoPriority.Low },
            new TodoItem { Title = "Todo 2", Priority = TodoPriority.High },
            new TodoItem { Title = "Todo 3", Priority = TodoPriority.Medium }
        };

        _context.TodoItems.AddRange(todos);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(t => t.Title == "Todo 1");
        result.Should().Contain(t => t.Title == "Todo 2");
        result.Should().Contain(t => t.Title == "Todo 3");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnTodo()
    {
        // Arrange
        var todo = new TodoItem
        {
            Title = "Test Todo",
            Description = "Test Description",
            Priority = TodoPriority.Medium
        };

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(todo.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(todo.Id);
        result.Title.Should().Be("Test Todo");
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingTodo()
    {
        // Arrange
        var todo = new TodoItem
        {
            Title = "Original Title",
            Description = "Original Description",
            Priority = TodoPriority.Low
        };

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        // Modify the todo
        todo.Title = "Updated Title";
        todo.Description = "Updated Description";
        todo.Priority = TodoPriority.High;
        todo.UpdatedAt = DateTime.UtcNow;

        // Act
        var result = await _repository.UpdateAsync(todo);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Description");
        result.Priority.Should().Be(TodoPriority.High);
        result.UpdatedAt.Should().NotBeNull();

        var dbTodo = await _context.TodoItems.FindAsync(todo.Id);
        dbTodo!.Title.Should().Be("Updated Title");
        dbTodo.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTodoFromDatabase()
    {
        // Arrange
        var todo = new TodoItem
        {
            Title = "Todo to Delete",
            Priority = TodoPriority.Medium
        };

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();
        var todoId = todo.Id;

        // Act
        await _repository.DeleteAsync(todoId);

        // Assert
        var dbTodo = await _context.TodoItems.FindAsync(todoId);
        dbTodo.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var todo = new TodoItem
        {
            Title = "Existing Todo",
            Priority = TodoPriority.Low
        };

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(todo.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByCompletionStatusAsync_WithCompletedTodos_ShouldReturnOnlyCompleted()
    {
        // Arrange
        var todos = new[]
        {
            new TodoItem { Title = "Completed Todo 1", IsCompleted = true, Priority = TodoPriority.Low },
            new TodoItem { Title = "Incomplete Todo", IsCompleted = false, Priority = TodoPriority.Medium },
            new TodoItem { Title = "Completed Todo 2", IsCompleted = true, Priority = TodoPriority.High }
        };

        _context.TodoItems.AddRange(todos);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCompletionStatusAsync(true);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(t => t.IsCompleted.Should().BeTrue());
        result.Should().Contain(t => t.Title == "Completed Todo 1");
        result.Should().Contain(t => t.Title == "Completed Todo 2");
    }

    [Fact]
    public async Task GetByCompletionStatusAsync_WithIncompleteTodos_ShouldReturnOnlyIncomplete()
    {
        // Arrange
        var todos = new[]
        {
            new TodoItem { Title = "Completed Todo", IsCompleted = true, Priority = TodoPriority.Low },
            new TodoItem { Title = "Incomplete Todo 1", IsCompleted = false, Priority = TodoPriority.Medium },
            new TodoItem { Title = "Incomplete Todo 2", IsCompleted = false, Priority = TodoPriority.High }
        };

        _context.TodoItems.AddRange(todos);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCompletionStatusAsync(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(t => t.IsCompleted.Should().BeFalse());
        result.Should().Contain(t => t.Title == "Incomplete Todo 1");
        result.Should().Contain(t => t.Title == "Incomplete Todo 2");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 