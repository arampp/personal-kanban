using PersonalKanban;
using PersonalKanban.Domain.Card;
using PersonalKanban.Domain.Column;

namespace PersonalKanbanTest;

public class ColumnNotificationHandlerTests
{
    [Fact]
    public void Adds_column_to_read_model()
    {
        ColumnCreated notification = new(Guid.NewGuid(), "Test Column", Guid.NewGuid());
        var readModel = new ColumnsReadModel();
        var handler = new ColumnNotificationHandler(readModel);

        handler.Handle(notification, CancellationToken.None);

        readModel.Columns.Should().ContainSingle();
        readModel.Columns.Single().Id.Should().Be(notification.Id);
        readModel.Columns.Single().Title.Should().Be(notification.Title);
    }
    [Fact]

    public void Adds_created_card_to_column()
    {
        var readModel = new ColumnsReadModel();
        var column = new Column
        {
            Id = Guid.NewGuid(),
            Title = "Test Column",
        };
        readModel.Add(column);
        var handler = new ColumnNotificationHandler(readModel);

        var cardCreated = new CardCreated(Guid.NewGuid(), "abc", "xyz", column.Id);
        handler.Handle(cardCreated, CancellationToken.None);

        readModel.Columns.Single().Cards.Single().Should().Be(cardCreated.Id);
    }
}
