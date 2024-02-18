using MediatR;

using NEventStore;

namespace PersonalKanban.Domain.Card;

public record CreateCard(string Title, string? Description, Guid ColumnId) : IRequest;
public record CardCreated(Guid Id, string Title, string Description, Guid ColumnId) : INotification;

public class CreateCardCommandHandler(IMediator mediator, IStoreEvents store) : IRequestHandler<CreateCard>
{

    public async Task Handle(CreateCard request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new RequestFailed($"'${nameof(request.Title)}' must not be empty");
        }
        var @event = new CardCreated(Guid.NewGuid(), request.Title, request.Description ?? "", request.ColumnId);
        cancellationToken.ThrowIfCancellationRequested();
        using (var stream = store.CreateStream("card", @event.Id))
        {
            stream.Add(new EventMessage { Body = @event });
            stream.CommitChanges(Guid.NewGuid());
        }
        await mediator.Publish(@event, cancellationToken);
    }
}