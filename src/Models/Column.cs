using LanguageExt;
using static LanguageExt.Prelude;

using MediatR;

using PersonalKanban.Domain.Card;
using PersonalKanban.Domain.Column;

namespace PersonalKanban;

public class Column
{
    private readonly IList<Guid> _cards = [];
    required public Guid Id { get; init; }
    required public string Title { get; init; }
    public IEnumerable<Guid> Cards => _cards;

    public void AddCard(Guid card)
    {
        _cards.Add(card);
    }

    public void RemoveCard(Guid card)
    {
        _cards.Remove(card);
    }

    internal void InsertCard(Guid card, int index)
    {
        _cards.Insert(index, card);
    }
}

public class ColumnNotificationHandler(ColumnsReadModel columnsReadModel) :
 INotificationHandler<ColumnCreated>,
 INotificationHandler<CardCreated>,
 INotificationHandler<CardMoved>
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

    public Task Handle(CardMoved notification, CancellationToken cancellationToken)
    {
        var source = columnsReadModel.Columns.Single(c => c.Id == notification.SourceColumn);
        var target = columnsReadModel.Columns.Single(c => c.Id == notification.TargetColumn);
        source.RemoveCard(notification.Card);
        target.InsertCard(notification.Card, notification.Position);
        return Task.CompletedTask;
    }
}

public interface IColumnsProvider
{
    IEnumerable<Column> Columns { get; }
    Option<Column> GetById(Guid id);
}

public class ColumnsService(ColumnsReadModel columnsReadModel) : IColumnsProvider
{
    public IEnumerable<Column> Columns => columnsReadModel.Columns;

    public Option<Column> GetById(Guid id)
    {
        var result = columnsReadModel.Columns.SingleOrDefault(c => c.Id == id);
        if (result == null)
        {
            return None;
        }
        return Some(result);
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
