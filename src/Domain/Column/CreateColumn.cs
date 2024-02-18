using MediatR;

using PersonalKanban.Domain.Board;

namespace PersonalKanban.Domain.Column;


public record CreateColumn(string Title, Guid boardId) : IRequest;
public record ColumnCreated(Guid Id, string Title, Guid BoardId) : INotification;

public class CreateColumnCommandHandler(IPublisher publisher, BoardsReadModel boards) : IRequestHandler<CreateColumn>
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
        await publisher.Publish(new ColumnCreated(Guid.NewGuid(), request.Title, request.boardId), cancellationToken);
    }
}
