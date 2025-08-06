using TestCorp.Application.DTOs;
using MediatR;

namespace TestCorp.Application.Commands;

public class CreateTodoCommand : IRequest<TodoItemDto>
{
    public CreateTodoItemDto Todo { get; set; }

    public CreateTodoCommand(CreateTodoItemDto todo)
    {
        Todo = todo;
    }
} 