using TestCorp.Application.DTOs;
using MediatR;

namespace TestCorp.Application.Commands;

public class UpdateTodoCommand : IRequest<TodoItemDto?>
{
    public int Id { get; set; }
    public UpdateTodoItemDto Todo { get; set; }

    public UpdateTodoCommand(int id, UpdateTodoItemDto todo)
    {
        Id = id;
        Todo = todo;
    }
} 