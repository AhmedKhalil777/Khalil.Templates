using AutoMapper;
using CleanKhalil.Application.DTOs;
using CleanKhalil.Application.Queries;
using CleanKhalil.Domain.Interfaces;
using MediatR;

namespace CleanKhalil.Application.Handlers;

public class GetTodoByIdQueryHandler : IRequestHandler<GetTodoByIdQuery, TodoItemDto?>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;

    public GetTodoByIdQueryHandler(ITodoRepository todoRepository, IMapper mapper)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
    }

    public async Task<TodoItemDto?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAsync(request.Id);
        return todo != null ? _mapper.Map<TodoItemDto>(todo) : null;
    }
} 