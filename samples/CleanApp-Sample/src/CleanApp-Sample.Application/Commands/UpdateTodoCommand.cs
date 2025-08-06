using Sample Company.Application.DTOs;
using MediatR;

namespace Sample Company.Application.Commands;

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