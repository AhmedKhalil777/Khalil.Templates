using Sample Company.Application.DTOs;
using MediatR;

namespace Sample Company.Application.Queries;

public class GetTodoByIdQuery : IRequest<TodoItemDto?>
{
    public int Id { get; set; }

    public GetTodoByIdQuery(int id)
    {
        Id = id;
    }
} 