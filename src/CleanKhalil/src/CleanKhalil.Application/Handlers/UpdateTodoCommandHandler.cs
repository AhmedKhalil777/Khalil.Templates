using AutoMapper;
using CleanKhalil.Application.Commands;
using CleanKhalil.Application.DTOs;
using CleanKhalil.Domain.Interfaces;
using MediatR;

namespace CleanKhalil.Application.Handlers;

public class UpdateTodoCommandHandler : IRequestHandler<UpdateTodoCommand, TodoItemDto?>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;

    public UpdateTodoCommandHandler(ITodoRepository todoRepository, IMapper mapper)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
    }

    public async Task<TodoItemDto?> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var existingTodo = await _todoRepository.GetByIdAsync(request.Id);
        if (existingTodo == null)
        {
            return null;
        }

        _mapper.Map(request.Todo, existingTodo);
        existingTodo.UpdatedAt = DateTime.UtcNow;

        var updatedTodo = await _todoRepository.UpdateAsync(existingTodo);
        return _mapper.Map<TodoItemDto>(updatedTodo);
    }
} 