using AutoMapper;
using Sample Company.Application.Commands;
using Sample Company.Application.DTOs;
using Sample Company.Domain.Entities;
using Sample Company.Domain.Interfaces;
using MediatR;

namespace Sample Company.Application.Handlers;

public class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, TodoItemDto>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;

    public CreateTodoCommandHandler(ITodoRepository todoRepository, IMapper mapper)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
    }

    public async Task<TodoItemDto> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todoItem = _mapper.Map<TodoItem>(request.Todo);
        var createdTodo = await _todoRepository.CreateAsync(todoItem);
        return _mapper.Map<TodoItemDto>(createdTodo);
    }
} 