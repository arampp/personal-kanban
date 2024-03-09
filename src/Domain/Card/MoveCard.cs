using MediatR;

using NEventStore;

namespace PersonalKanban.Domain.Card;

public record MoveCard : IRequest
{
    required public Guid Card { get; init; }
    required public Guid SourceColumn { get; init; }
    required public Guid TargetColumn { get; init; }
    required public int Position { get; init; }
}

public record CardMoved : INotification
{
    required public Guid Card { get; init; }
    required public Guid SourceColumn { get; init; }
    required public Guid TargetColumn { get; init; }
    required public int Position { get; init; }
}

public class CardMovedHandler : IRequestHandler<MoveCard>
{
    private readonly IStoreEvents _store;
    private readonly IMediator _mediator;
    private readonly IColumnsProvider _columns;

    public CardMovedHandler(IStoreEvents store, IMediator mediator, IColumnsProvider columns)
    {
        _store = store;
        _mediator = mediator;
        _columns = columns;
    }

    public async Task Handle(MoveCard request, CancellationToken cancellationToken)
    {
        // var stream = _store.OpenStream(request.Card);
        // var card = stream.CommittedEvents.Last().Body as Card;
        // stream.Add(new EventMessage { Body = new CardMoved
        // {
        //     Card = request.Card,
        //     SourceColumn = request.SourceColumn,
        //     TargetColumn = request.TargetColumn,
        //     Position = request.Position
        // }});
        // await _store.Advanced.CommitChanges(stream);
        var sourceColumn = _columns.GetById(request.SourceColumn).IfNone(() => throw new NotFound($"SourceColumn not found (Column-ID ${request.SourceColumn})"));
        var targetColumn = _columns.GetById(request.TargetColumn).IfNone(() => throw new NotFound($"TargetColumn not found (Column-ID ${request.TargetColumn})"));
        var card = sourceColumn.Cards.SingleOrDefault(c => c == request.Card);
        if (card == default)
        {
            throw new NotFound($"Card not found in source column (Card-ID ${request.Card})");
        }


        var highestTargetColumnIndex = targetColumn.Cards.Count();

        var actualPosition = request.Position;
        actualPosition = actualPosition < 0 ? 0 : actualPosition;
        actualPosition = actualPosition > highestTargetColumnIndex ? highestTargetColumnIndex : actualPosition;
        var @event = new CardMoved
        {
            Card = request.Card,
            SourceColumn = request.SourceColumn,
            TargetColumn = request.TargetColumn,
            Position = actualPosition
        };
        cancellationToken.ThrowIfCancellationRequested();
        using (var stream = _store.OpenStream("card", card))
        {
            stream.Add(new EventMessage { Body = @event });
            stream.CommitChanges(Guid.NewGuid());
        }
        await _mediator.Publish(@event, cancellationToken);
    }
}