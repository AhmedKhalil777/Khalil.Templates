using Sample Company.Application.DTOs;
using MediatR;

namespace Sample Company.Application.Commands;

public class CreateTodoCommand : IRequest<TodoItemDto>
{
    public CreateTodoItemDto Todo { get; set; }

    public CreateTodoCommand(CreateTodoItemDto todo)
    {
        Todo = todo;
    }
} 