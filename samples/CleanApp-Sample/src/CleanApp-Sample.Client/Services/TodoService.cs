using Sample Company.Application.DTOs;
using System.Text.Json;
using System.Text;

namespace Sample Company.Client.Services;

public class TodoService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public TodoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<IEnumerable<TodoItemDto>> GetAllTodosAsync()
    {
        var response = await _httpClient.GetAsync("api/todo");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<TodoItemDto>>(content, _jsonOptions) ?? Enumerable.Empty<TodoItemDto>();
    }

    public async Task<TodoItemDto?> GetTodoAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/todo/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TodoItemDto>(content, _jsonOptions);
    }

    public async Task<TodoItemDto> CreateTodoAsync(CreateTodoItemDto createTodo)
    {
        var json = JsonSerializer.Serialize(createTodo, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/todo", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TodoItemDto>(responseContent, _jsonOptions)!;
    }

    public async Task<TodoItemDto?> UpdateTodoAsync(int id, UpdateTodoItemDto updateTodo)
    {
        var json = JsonSerializer.Serialize(updateTodo, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync($"api/todo/{id}", content);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TodoItemDto>(responseContent, _jsonOptions);
    }

    public async Task<bool> DeleteTodoAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/todo/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<TodoItemDto?> CompleteTodoAsync(int id)
    {
        var response = await _httpClient.PatchAsync($"api/todo/{id}/complete", null);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TodoItemDto>(responseContent, _jsonOptions);
    }
} 