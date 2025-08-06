using CleanKhalil.Application.Commands;
using CleanKhalil.Domain.Interfaces;
using MediatR;

namespace CleanKhalil.Application.Handlers;

public class DeleteTodoCommandHandler : IRequestHandler<DeleteTodoCommand, bool>
{
    private readonly ITodoRepository _todoRepository;

    public DeleteTodoCommandHandler(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var exists = await _todoRepository.ExistsAsync(request.Id);
        if (!exists)
        {
            return false;
        }

        await _todoRepository.DeleteAsync(request.Id);
        return true;
    }
} 