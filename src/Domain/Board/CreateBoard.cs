using MediatR;

using NEventStore;

namespace PersonalKanban.Domain.Board;


public record CreateBoard(string Title, string? Description) : IRequest;
public record BoardCreated(Guid Id, string Title, string Description) : INotification;

public class CreateBoardCommandHandler(IPublisher publisher, IStoreEvents store) : IRequestHandler<CreateBoard>
{

    public async Task Handle(CreateBoard request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new RequestFailed($"'${nameof(request.Title)}' must not be empty");
        }
        cancellationToken.ThrowIfCancellationRequested();
        var boardCreated = new BoardCreated(Guid.NewGuid(), request.Title, request.Description ?? "");
        using (var stream = store.CreateStream(boardCreated.Id))
        {
            stream.Add(new EventMessage { Body = boardCreated });
            stream.CommitChanges(Guid.NewGuid());
        }
        await publisher.Publish(boardCreated, cancellationToken);
    }
}