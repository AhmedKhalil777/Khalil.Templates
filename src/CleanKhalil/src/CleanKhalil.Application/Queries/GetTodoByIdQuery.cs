using CleanKhalil.Application.DTOs;
using MediatR;

namespace CleanKhalil.Application.Queries;

public class GetTodoByIdQuery : IRequest<TodoItemDto?>
{
    public int Id { get; set; }

    public GetTodoByIdQuery(int id)
    {
        Id = id;
    }
} 