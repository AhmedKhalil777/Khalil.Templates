using Sample Company.Application.DTOs;
using MediatR;

namespace Sample Company.Application.Queries;

public class GetAllTodosQuery : IRequest<IEnumerable<TodoItemDto>>
{
} 