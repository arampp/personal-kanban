using MediatR;

namespace PersonalKanban.Domain.Card;

public record CreateCard(string Title, string? Description, Guid ColumnId) : IRequest;
public record CardCreated(Guid Id, string Title, string Description, Guid ColumnId) : INotification;

public class CreateCardCommandHandler(IMediator mediator) : IRequestHandler<CreateCard>
{

    public async Task Handle(CreateCard request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new RequestFailed($"'${nameof(request.Title)}' must not be empty");
        }
        await mediator.Publish(new CardCreated(Guid.NewGuid(), request.Title, request.Description ?? "", request.ColumnId), cancellationToken);
    }
}