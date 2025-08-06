using Sample Company.Application.DTOs;
using MediatR;

namespace Sample Company.Application.Commands;

public class CompleteTodoCommand : IRequest<TodoItemDto?>
{
    public int Id { get; set; }

    public CompleteTodoCommand(int id)
    {
        Id = id;
    }
} 