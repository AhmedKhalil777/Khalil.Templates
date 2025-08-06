using CleanKhalil.Application.DTOs;
using MediatR;

namespace CleanKhalil.Application.Commands;

public class CompleteTodoCommand : IRequest<TodoItemDto?>
{
    public int Id { get; set; }

    public CompleteTodoCommand(int id)
    {
        Id = id;
    }
} 