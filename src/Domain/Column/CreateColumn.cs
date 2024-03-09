using MediatR;

using NEventStore;

namespace PersonalKanban.Domain.Column;


public record CreateColumn(string Title, Guid boardId) : IRequest;
public record ColumnCreated(Guid Id, string Title, Guid BoardId) : INotification;

public class CreateColumnCommandHandler(IPublisher publisher, BoardsReadModel boards, IStoreEvents store) : IRequestHandler<CreateColumn>
{

    public async Task Handle(CreateColumn request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new RequestFailed($"'${nameof(request.Title)}' must not be empty");
        }
        if (!boards.Boards.Select(b => b.Id).Contains(request.boardId))
        {
            throw new RequestFailed($"Board with id {request.boardId} does not exist");
        }
        cancellationToken.ThrowIfCancellationRequested();
        var columnCreated = new ColumnCreated(Guid.NewGuid(), request.Title, request.boardId);
        using (var stream = store.CreateStream("column", columnCreated.Id))
        {
            stream.Add(new EventMessage { Body = columnCreated });
            stream.CommitChanges(Guid.NewGuid());
        }

        await publisher.Publish(columnCreated, cancellationToken);
    }
}
