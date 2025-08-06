using TestCorp.Application.DTOs;
using MediatR;

namespace TestCorp.Application.Queries;

public class GetAllTodosQuery : IRequest<IEnumerable<TodoItemDto>>
{
} 