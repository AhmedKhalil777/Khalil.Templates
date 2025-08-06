using TestCorp.Application.DTOs;
using MediatR;

namespace TestCorp.Application.Queries;

public class GetTodoByIdQuery : IRequest<TodoItemDto?>
{
    public int Id { get; set; }

    public GetTodoByIdQuery(int id)
    {
        Id = id;
    }
} 