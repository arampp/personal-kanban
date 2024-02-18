using MediatR;

namespace PersonalKanban.Domain.Board;


public record CreateBoard(string Title, string? Description) : IRequest;
public record BoardCreated(Guid Id, string Title, string Description) : INotification;

public class CreateBoardCommandHandler(IPublisher publisher) : IRequestHandler<CreateBoard>
{

    public async Task Handle(CreateBoard request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new RequestFailed($"'${nameof(request.Title)}' must not be empty");
        }
        await publisher.Publish(new BoardCreated(Guid.NewGuid(), request.Title, request.Description ?? ""), cancellationToken);
    }
}