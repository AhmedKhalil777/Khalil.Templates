using TestCorp.Application.DTOs;
using MediatR;

namespace TestCorp.Application.Commands;

public class CompleteTodoCommand : IRequest<TodoItemDto?>
{
    public int Id { get; set; }

    public CompleteTodoCommand(int id)
    {
        Id = id;
    }
} 