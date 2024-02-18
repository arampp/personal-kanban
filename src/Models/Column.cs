using MediatR;

using PersonalKanban.Domain.Card;
using PersonalKanban.Domain.Column;

namespace PersonalKanban;

public class Column
{
    private readonly ICollection<Guid> _cards = [];
    required public Guid Id { get; init; }
    required public string Title { get; init; }
    public IEnumerable<Guid> Cards => _cards;

    public void AddCard(Guid card)
    {
        _cards.Add(card);
    }
}

public class ColumnNotificationHandler(ColumnsReadModel columnsReadModel) :
 INotificationHandler<ColumnCreated>,
 INotificationHandler<CardCreated>
{
    public Task Handle(ColumnCreated notification, CancellationToken cancellationToken)
    {
        columnsReadModel.Add(new Column
        {
            Id = notification.Id,
            Title = notification.Title,
        });
        return Task.CompletedTask;
    }

    public Task Handle(CardCreated cardCreated, CancellationToken none)
    {
        columnsReadModel.Columns.Single(c => c.Id == cardCreated.ColumnId).AddCard(cardCreated.Id);
        return Task.CompletedTask;
    }
}

public interface IColumnsProvider
{
    IEnumerable<Column> Columns { get; }
    Column GetById(Guid id);
}

public class ColumnsService(ColumnsReadModel columnsReadModel) : IColumnsProvider
{
    public IEnumerable<Column> Columns => columnsReadModel.Columns;

    public Column GetById(Guid id)
    {
        return columnsReadModel.Columns.Single(c => c.Id == id);
    }
}

public class ColumnsReadModel
{
    private readonly ICollection<Column> _columns = [];
    public IEnumerable<Column> Columns => _columns;
    public void Add(Column column)
    {
        _columns.Add(column);
    }
}
