using AutoMapper;
using CleanKhalil.Application.Commands;
using CleanKhalil.Application.DTOs;
using CleanKhalil.Domain.Entities;
using CleanKhalil.Domain.Interfaces;
using MediatR;

namespace CleanKhalil.Application.Handlers;

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