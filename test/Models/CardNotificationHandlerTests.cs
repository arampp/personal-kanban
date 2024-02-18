using PersonalKanban.Domain.Card;
using PersonalKanban.Models;

namespace PersonalKanbanTest.Models;

public class CardNotificationHandlerTests
{
    [Fact]
    public async Task Adds_card_to_collection_on_CardCreated()
    {
        var cardsCollection = new CardsReadModel();
        var handler = new CardNotificationHandler(cardsCollection);
        var notification = new CardCreated(Guid.NewGuid(), "Test", "Test description", Guid.NewGuid());

        await handler.Handle(notification, CancellationToken.None);

        cardsCollection.Cards.Should().ContainSingle();
        var card = cardsCollection.Cards.Single();
        card.Id.Should().Be(notification.Id);
        card.Title.Should().Be("Test");
        card.Description.Should().Be("Test description");
    }

}
