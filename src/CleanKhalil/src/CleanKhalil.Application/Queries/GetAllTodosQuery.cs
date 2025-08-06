using CleanKhalil.Application.DTOs;
using MediatR;

namespace CleanKhalil.Application.Queries;

public class GetAllTodosQuery : IRequest<IEnumerable<TodoItemDto>>
{
} 