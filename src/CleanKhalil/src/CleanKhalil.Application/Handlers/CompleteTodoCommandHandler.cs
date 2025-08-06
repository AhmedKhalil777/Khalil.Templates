using AutoMapper;
using CleanKhalil.Application.Commands;
using CleanKhalil.Application.DTOs;
using CleanKhalil.Domain.Interfaces;
using MediatR;

namespace CleanKhalil.Application.Handlers;

public class CompleteTodoCommandHandler : IRequestHandler<CompleteTodoCommand, TodoItemDto?>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;

    public CompleteTodoCommandHandler(ITodoRepository todoRepository, IMapper mapper)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
    }

    public async Task<TodoItemDto?> Handle(CompleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAsync(request.Id);
        if (todo == null)
        {
            return null;
        }

        todo.MarkAsCompleted();
        var updatedTodo = await _todoRepository.UpdateAsync(todo);
        return _mapper.Map<TodoItemDto>(updatedTodo);
    }
} 