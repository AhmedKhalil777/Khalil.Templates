using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoAPI_Sample.Data;
using TodoAPI_Sample.Models;

namespace TodoAPI_Sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly TodoDbContext _context;
    private readonly ILogger<TodoController> _logger;

    public TodoController(TodoDbContext context, ILogger<TodoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos(
        [FromQuery] bool? isCompleted = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        try
        {
            var query = _context.TodoItems.AsQueryable();

            if (isCompleted.HasValue)
                query = query.Where(t => t.IsCompleted == isCompleted.Value);

            var todos = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(Math.Min(take, 100)) // Max 100 items per request
                .ToListAsync();

            return Ok(todos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving todos");
            return StatusCode(500, "An error occurred while retrieving todos");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetTodo(int id)
    {
        try
        {
            var todo = await _context.TodoItems.FindAsync(id);

            if (todo == null)
                return NotFound($"Todo with ID {id} not found");

            return Ok(todo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving todo with ID {TodoId}", id);
            return StatusCode(500, "An error occurred while retrieving the todo");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TodoItem>> CreateTodo(TodoItem todo)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            todo.Id = 0; // Ensure ID is 0 for new entities
            todo.CreatedAt = DateTime.UtcNow;
            todo.UpdatedAt = null;
            todo.CompletedAt = null;

            _context.TodoItems.Add(todo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating todo");
            return StatusCode(500, "An error occurred while creating the todo");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodo(int id, TodoItem todo)
    {
        try
        {
            if (id != todo.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingTodo = await _context.TodoItems.FindAsync(id);
            if (existingTodo == null)
                return NotFound($"Todo with ID {id} not found");

            existingTodo.Title = todo.Title;
            existingTodo.Description = todo.Description;
            existingTodo.UpdatedAt = DateTime.UtcNow;

            // Handle completion status change
            if (existingTodo.IsCompleted != todo.IsCompleted)
            {
                existingTodo.IsCompleted = todo.IsCompleted;
                existingTodo.CompletedAt = todo.IsCompleted ? DateTime.UtcNow : null;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await TodoExists(id))
                return NotFound();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating todo with ID {TodoId}", id);
            return StatusCode(500, "An error occurred while updating the todo");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        try
        {
            var todo = await _context.TodoItems.FindAsync(id);
            if (todo == null)
                return NotFound($"Todo with ID {id} not found");

            _context.TodoItems.Remove(todo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting todo with ID {TodoId}", id);
            return StatusCode(500, "An error occurred while deleting the todo");
        }
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> CompleteTodo(int id)
    {
        try
        {
            var todo = await _context.TodoItems.FindAsync(id);
            if (todo == null)
                return NotFound($"Todo with ID {id} not found");

            todo.IsCompleted = true;
            todo.CompletedAt = DateTime.UtcNow;
            todo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing todo with ID {TodoId}", id);
            return StatusCode(500, "An error occurred while completing the todo");
        }
    }

    private async Task<bool> TodoExists(int id)
    {
        return await _context.TodoItems.AnyAsync(e => e.Id == id);
    }
} 