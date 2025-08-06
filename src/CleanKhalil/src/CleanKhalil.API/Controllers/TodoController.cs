using CleanKhalil.Application.Commands;
using CleanKhalil.Application.DTOs;
using CleanKhalil.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanKhalil.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TodoController> _logger;

    public TodoController(IMediator mediator, ILogger<TodoController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetTodos()
    {
        try
        {
            var todos = await _mediator.Send(new GetAllTodosQuery());
            return Ok(todos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching todos");
            return StatusCode(500, "Internal server error occurred while fetching todos");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemDto>> GetTodo(int id)
    {
        try
        {
            var todo = await _mediator.Send(new GetTodoByIdQuery(id));
            
            if (todo == null)
            {
                return NotFound($"Todo with ID {id} not found");
            }

            return Ok(todo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching todo with ID {TodoId}", id);
            return StatusCode(500, "Internal server error occurred while fetching todo");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TodoItemDto>> CreateTodo([FromBody] CreateTodoItemDto createTodoDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdTodo = await _mediator.Send(new CreateTodoCommand(createTodoDto));
            return CreatedAtAction(nameof(GetTodo), new { id = createdTodo.Id }, createdTodo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating todo");
            return StatusCode(500, "Internal server error occurred while creating todo");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TodoItemDto>> UpdateTodo(int id, [FromBody] UpdateTodoItemDto updateTodoDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedTodo = await _mediator.Send(new UpdateTodoCommand(id, updateTodoDto));
            
            if (updatedTodo == null)
            {
                return NotFound($"Todo with ID {id} not found");
            }

            return Ok(updatedTodo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating todo with ID {TodoId}", id);
            return StatusCode(500, "Internal server error occurred while updating todo");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTodo(int id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteTodoCommand(id));
            
            if (!result)
            {
                return NotFound($"Todo with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting todo with ID {TodoId}", id);
            return StatusCode(500, "Internal server error occurred while deleting todo");
        }
    }

    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<TodoItemDto>> CompleteTodo(int id)
    {
        try
        {
            var completedTodo = await _mediator.Send(new CompleteTodoCommand(id));
            
            if (completedTodo == null)
            {
                return NotFound($"Todo with ID {id} not found");
            }

            return Ok(completedTodo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing todo with ID {TodoId}", id);
            return StatusCode(500, "Internal server error occurred while completing todo");
        }
    }
} 