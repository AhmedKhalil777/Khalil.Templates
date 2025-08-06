using AutoMapper;
using TestCorp.Application.DTOs;
using TestCorp.Application.Queries;
using TestCorp.Domain.Interfaces;
using MediatR;

namespace TestCorp.Application.Handlers;

public class GetAllTodosQueryHandler : IRequestHandler<GetAllTodosQuery, IEnumerable<TodoItemDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;

    public GetAllTodosQueryHandler(ITodoRepository todoRepository, IMapper mapper)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TodoItemDto>> Handle(GetAllTodosQuery request, CancellationToken cancellationToken)
    {
        var todos = await _todoRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<TodoItemDto>>(todos);
    }
} 