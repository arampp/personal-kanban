using MediatR;

using PersonalKanban.Domain.Board;
using PersonalKanban.Domain.Column;

namespace PersonalKanban;

public class Board
{
    private readonly ICollection<Guid> _columns = [];
    required public Guid Id { get; init; }
    required public string Title { get; init; }
    public IEnumerable<Guid> Columns => _columns;

    public void AddColumn(Guid column)
    {
        _columns.Add(column);
    }
}

public class BoardNotificationHandler(BoardsReadModel boards) :
 INotificationHandler<BoardCreated>,
 INotificationHandler<ColumnCreated>
{
    public Task Handle(BoardCreated notification, CancellationToken cancellationToken)
    {
        boards.Add(new Board
        {
            Id = notification.Id,
            Title = notification.Title,
        });
        return Task.CompletedTask;
    }

    public Task Handle(ColumnCreated notification, CancellationToken cancellationToken)
    {
        var board = boards.Boards.First(b => b.Id == notification.BoardId);
        board?.AddColumn(notification.Id);
        return Task.CompletedTask;
    }
}

public interface IBoardsProvider
{
    IEnumerable<Board> Boards { get; }
}

public class BoardsService(BoardsReadModel boards) : IBoardsProvider
{
    public IEnumerable<Board> Boards => boards.Boards;
}

public class BoardsReadModel
{
    private readonly ICollection<Board> _boards = [];
    public IEnumerable<Board> Boards => _boards;
    public void Add(Board board)
    {
        _boards.Add(board);
    }
}