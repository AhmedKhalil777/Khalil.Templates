using APISqlKhalil.Data;
using APISqlKhalil.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace APISqlKhalil.Tests;

public class TodoControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TodoControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TodoContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<TodoContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetTodos_ShouldReturnEmptyList_WhenNoTodosExist()
    {
        // Act
        var response = await _client.GetAsync("/api/todo");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var todos = JsonSerializer.Deserialize<List<TodoItem>>(content, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        todos.Should().NotBeNull();
        todos.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateTodo_ShouldReturnCreatedTodo()
    {
        // Arrange
        var newTodo = new TodoItem
        {
            Title = "Test Todo",
            Description = "Test Description",
            IsCompleted = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todo", newTodo);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var createdTodo = JsonSerializer.Deserialize<TodoItem>(content, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        createdTodo.Should().NotBeNull();
        createdTodo!.Id.Should().BeGreaterThan(0);
        createdTodo.Title.Should().Be("Test Todo");
        createdTodo.Description.Should().Be("Test Description");
        createdTodo.IsCompleted.Should().BeFalse();
        createdTodo.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetTodoById_ShouldReturnTodo_WhenTodoExists()
    {
        // Arrange
        var newTodo = new TodoItem
        {
            Title = "Test Todo for Get",
            Description = "Test Description",
            IsCompleted = false
        };

        var createResponse = await _client.PostAsJsonAsync("/api/todo", newTodo);
        createResponse.EnsureSuccessStatusCode();
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdTodo = JsonSerializer.Deserialize<TodoItem>(createdContent, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        // Act
        var response = await _client.GetAsync($"/api/todo/{createdTodo!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var retrievedTodo = JsonSerializer.Deserialize<TodoItem>(content, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        retrievedTodo.Should().NotBeNull();
        retrievedTodo!.Id.Should().Be(createdTodo.Id);
        retrievedTodo.Title.Should().Be("Test Todo for Get");
    }

    [Fact]
    public async Task UpdateTodo_ShouldReturnUpdatedTodo()
    {
        // Arrange
        var newTodo = new TodoItem
        {
            Title = "Original Title",
            Description = "Original Description",
            IsCompleted = false
        };

        var createResponse = await _client.PostAsJsonAsync("/api/todo", newTodo);
        createResponse.EnsureSuccessStatusCode();
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdTodo = JsonSerializer.Deserialize<TodoItem>(createdContent, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        var updateTodo = new TodoItem
        {
            Id = createdTodo!.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            IsCompleted = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todo/{createdTodo.Id}", updateTodo);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var updatedTodo = JsonSerializer.Deserialize<TodoItem>(content, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        updatedTodo.Should().NotBeNull();
        updatedTodo!.Title.Should().Be("Updated Title");
        updatedTodo.Description.Should().Be("Updated Description");
        updatedTodo.IsCompleted.Should().BeTrue();
        updatedTodo.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteTodo_ShouldReturnNoContent()
    {
        // Arrange
        var newTodo = new TodoItem
        {
            Title = "Todo to Delete",
            Description = "Will be deleted",
            IsCompleted = false
        };

        var createResponse = await _client.PostAsJsonAsync("/api/todo", newTodo);
        createResponse.EnsureSuccessStatusCode();
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdTodo = JsonSerializer.Deserialize<TodoItem>(createdContent, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        // Act
        var response = await _client.DeleteAsync($"/api/todo/{createdTodo!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // Verify todo is deleted
        var getResponse = await _client.GetAsync($"/api/todo/{createdTodo.Id}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
} 