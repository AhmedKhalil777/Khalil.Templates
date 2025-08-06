using MediatR;

namespace TestCorp.Application.Commands;

public class DeleteTodoCommand : IRequest<bool>
{
    public int Id { get; set; }

    public DeleteTodoCommand(int id)
    {
        Id = id;
    }
} 